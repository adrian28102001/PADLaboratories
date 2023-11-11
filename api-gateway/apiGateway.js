const express = require('express');
const multer = require('multer');
const {setupCache} = require('axios-cache-adapter');
const loadMonitor = require('./loadMonitor');
const ServiceDiscoveryChecker = require('./serviceDiscoveryChecker');
const RedisClient = require('./redisClient');
const ConfigManager = require('./configManager');
const serviceDiscovery = require('./serviceDiscovery');
const axios = require('axios');
const FormData = require('form-data');
const { promisify } = require('util');
const client = require('prom-client');
const CircuitBreaker = require('./circuitBreaker');

class ApiGateway {
    constructor() {
        this.configManager = new ConfigManager(process.env.NODE_ENV);
        this.config = this.configManager.getConfig();
        this.redisClient = new RedisClient(this.config.REDIS_CONFIG, 10000).getClient();
        this.getAsync = promisify(this.redisClient.get).bind(this.redisClient);
        this.cache = setupCache({ ...this.config.CACHE_CONFIG, redis: this.redisClient });
        this.serviceDiscoveryChecker = new ServiceDiscoveryChecker(this.config.SERVICE_DISCOVERY_URL, 10000);
        // Create a Prometheus Registry to register your custom metrics
        this.prometheusRegister = new client.Registry();
        // Register Prometheus default metrics
        client.collectDefaultMetrics({ register: this.prometheusRegister });
        this.app = express();
        this.upload = multer();
        this.setupMiddleware();
        this.setupPrometheusMetrics();
        this.setupRoutes();
    }

    setupMiddleware() {
        this.app.use(this.upload.any());
        this.app.use(loadMonitor);
        this.app.use(express.json());
    }

    setupPrometheusMetrics() {
        // Define custom metrics
        const customCounter = new client.Counter({
            name: 'api_gateway_custom_requests_total',
            help: 'Total number of custom requests in the API Gateway',
            labelNames: ['method'],
            registers: [this.prometheusRegister],
        });

        // Add a metric whenever a request is handled
        this.app.use((req, res, next) => {
            customCounter.labels(req.method).inc();
            next();
        });

        // Define a route for `/metrics` to expose the metrics
        this.app.get('/metrics', async (req, res) => {
            res.set('Content-Type', this.prometheusRegister.contentType);
            res.end(await this.prometheusRegister.metrics());
        });
    }

    async getNextServiceUrl(serviceName) {
        const serviceUrlsCacheKey = `service_urls_${serviceName}`;
        let serviceUrls;
        try {
            const cacheResult = await this.getAsync(serviceUrlsCacheKey);
            if (cacheResult) {
                serviceUrls = JSON.parse(cacheResult);
            } else {
                console.log(`No URLs found in cache for ${serviceName}. Invoking service discovery.`);
                serviceUrls = await serviceDiscovery(serviceName, this.redisClient); // Fetching new URLs from service discovery
                if (!serviceUrls || serviceUrls.length === 0) {
                    console.error(`Service discovery for ${serviceName} failed or returned empty list.`);
                    return null;
                }
                await this.redisClient.set(serviceUrlsCacheKey, JSON.stringify(serviceUrls), 'EX', this.config.CACHE_TTL); // Caching the discovered URLs
                console.log(`Service URLs for ${serviceName} cached:`, serviceUrls);
            }
        } catch (error) {
            console.error(`Error retrieving or parsing URLs for ${serviceName} from Redis cache:`, error);
            throw error; // Or handle it as per your error handling strategy
        }

        // Load balancing with round-robin strategy
        const serviceCounterKey = `service_counter_${serviceName}`;
        const currentCounter = await this.incrementCounter(serviceCounterKey); // Increment the counter for round-robin
        console.log(`Current round-robin counter for ${serviceName}:`, currentCounter);

        const serviceIndex = currentCounter % serviceUrls.length;
        const nextServiceUrl = serviceUrls[serviceIndex];
        console.log(`Redirecting to instance ${serviceIndex + 1} of ${serviceName}:`, nextServiceUrl);

        return nextServiceUrl;
    }


    async incrementCounter(key) {
        return new Promise((resolve, reject) => {
            this.redisClient.incr(key, (err, reply) => {
                if (err) {
                    reject(err);
                } else {
                    console.log(`Counter for ${key} incremented:`, reply);
                    resolve(reply);
                }
            });
        });
    }

    setupRoutes() {
        const circuitBreaker = new CircuitBreaker(30000); // Instantiate with a 30-second timeout

        this.app.get('/health', (req, res) => {
            if (this.serviceDiscoveryChecker.getStatus()) {
                res.status(200).send('API Gateway is healthy');
            } else {
                res.status(503).send('API Gateway is not ready yet. Service Discovery not available.');
            }
        });

        this.app.get('/clear-cache', async (req, res) => {
            await this.redisClient.flushdb();
            res.status(200).send("Redis cache cleared");
        });

        this.app.use('/', async (req, res) => {
            const serviceName = req.path.split('/')[1];
            const serviceCamelCaseName = `${serviceName.charAt(0).toLowerCase()}${serviceName.slice(1)}`;
            const serviceURL = await this.getNextServiceUrl(serviceCamelCaseName, this.redisClient);

            if (!serviceURL) {
                console.error(`No service URL available for ${serviceCamelCaseName}.`);
                return res.status(500).send('Service not found');
            }
            try {
                let config = {
                    method: req.method,
                    url: `${serviceURL}${req.path}`,
                };

                console.log(`Incoming request to ${req.method} ${req.path}`);

                if (req.is('multipart/form-data')) {
                    const formData = new FormData();
                    req.files.forEach(file => formData.append(file.fieldname, file.buffer, {
                        filename: file.originalname,
                        contentType: file.mimetype,
                    }));
                    Object.keys(req.body).forEach(key => formData.append(key, req.body[key]));
                    config.data = formData;
                } else {
                    config.data = req.body;
                }

                if (req.method.toLowerCase() !== 'get') {
                    config.adapter = undefined;
                } else {
                    config.adapter = this.cache.adapter;
                }

                // Call the service using the CircuitBreaker
                const response = await circuitBreaker.call(() => axios(config));

                console.log(`Forwarded request to service: ${serviceName} at URL: ${serviceURL}`);
                res.status(response.status).send(response.data);
            } catch (error) {
                let isReroute = false;
                // Check if the error code indicates a reroute situation
                if (error.response && error.response.status >= 500) {
                    isReroute = true;
                }

                if (error.response) {
                    console.error('Server responded with error:', error.response.data);
                    // Only send reroute status if it's the first time to avoid loops
                    if (isReroute && !req.headers['x-rerouted']) {
                        // Attempt a reroute by recursively calling the same endpoint
                        req.headers['x-rerouted'] = true; // Mark the request as rerouted
                        return this.app.handle(req, res);
                    }
                    return res.status(error.response.status).send(error.response.data);
                } else if (error.request) {
                    console.error('No response received:', error.request);
                } else {
                    console.error('Error calling service:', error.message);
                }
                res.status(500).send(error.message);
            }
        });
    }

    start() {
        this.app.listen(this.config.PORT, () => {
            console.log(`API Gateway is running on port ${this.config.PORT} in ${this.configManager.environment} environment`);
        });
    }
}

module.exports = ApiGateway;
