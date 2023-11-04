const express = require('express');
const multer = require('multer');
const { setupCache } = require('axios-cache-adapter');
const loadMonitor = require('./loadMonitor');
const ServiceDiscoveryChecker = require('./serviceDiscoveryChecker');
const RedisClient = require('./redisClient');
const ConfigManager = require('./configManager');
const serviceDiscovery = require('./serviceDiscovery');
const axios = require('axios');
const FormData = require('form-data');
const { promisify } = require('util');
const CircuitBreaker = require('opossum');

class ApiGateway {
    constructor() {
        this.configManager = new ConfigManager(process.env.NODE_ENV);
        this.config = this.configManager.getConfig();
        this.redisClient = new RedisClient(this.config.REDIS_CONFIG, 10000).getClient();
        this.getAsync = promisify(this.redisClient.get).bind(this.redisClient);
        this.cache = setupCache({ ...this.config.CACHE_CONFIG, redis: this.redisClient });
        this.serviceDiscoveryChecker = new ServiceDiscoveryChecker(this.config.SERVICE_DISCOVERY_URL, 10000);
        this.circuitBreakers = {};
        this.failedReroutes = {}; // Track failed reroutes per service
        this.rerouteThreshold = this.config.REROUTE_THRESHOLD; // Example threshold
        this.breakerOptions = {
            timeout: 5000, // If our function takes longer than 5 seconds, trigger a failure
            errorThresholdPercentage: 50, // When 50% of requests fail, trip the circuit
            resetTimeout: 30000 // After 30 seconds, try again.
        };
        this.app = express();
        this.upload = multer();
        this.setupMiddleware();
        this.setupRoutes();
    }

    getCircuitBreakerForService(serviceName) {
        if (!this.circuitBreakers[serviceName]) {
            this.circuitBreakers[serviceName] = new CircuitBreaker(serviceDiscovery, this.breakerOptions);
            this.circuitBreakers[serviceName].fallback(() => `Service ${serviceName} is currently unavailable.`);
            this.circuitBreakers[serviceName].on('open', () => console.log(`Circuit for ${serviceName} opened`));
            this.circuitBreakers[serviceName].on('close', () => console.log(`Circuit for ${serviceName} closed`));
        }
        return this.circuitBreakers[serviceName];
    }

    setupMiddleware() {
        this.app.use(this.upload.any());
        this.app.use(loadMonitor);
        this.app.use(express.json());
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
                const breaker = this.getCircuitBreakerForService(serviceName);
                serviceUrls = await breaker.fire(serviceName, this.redisClient);
                if (!serviceUrls || serviceUrls.length === 0) {
                    console.error(`Service discovery for ${serviceName} failed or returned an empty list.`);
                    return null;
                }
                await this.redisClient.set(serviceUrlsCacheKey, JSON.stringify(serviceUrls), 'EX', this.config.CACHE_TTL); // Caching the discovered URLs
                console.log(`Service URLs for ${serviceName} cached:`, serviceUrls);
            }
        } catch (error) {
            this.incrementFailedReroutes(serviceName);
            if (this.failedReroutes[serviceName] >= this.rerouteThreshold) {
                const breaker = this.getCircuitBreakerForService(serviceName);
                breaker.fallback(() => `Service ${serviceName} is currently unavailable.`);
                breaker.open();
                this.failedReroutes[serviceName] = 0; // Reset counter after tripping the circuit
            }

            console.error(`Error retrieving or parsing URLs for ${serviceName} from Redis cache:`, error);
            throw error; // Or handle it as per your error handling strategy
        }

        const serviceCounterKey = `service_counter_${serviceName}`;
        const currentCounter = await this.incrementCounter(serviceCounterKey);
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

    incrementFailedReroutes(serviceName) {
        if (!this.failedReroutes[serviceName]) {
            this.failedReroutes[serviceName] = 0;
        }
        this.failedReroutes[serviceName] += 1;
    }

    resetFailedReroutes(serviceName) {
        this.failedReroutes[serviceName] = 0;
    }

    setupRoutes() {
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
            const serviceURL = await this.getNextServiceUrl(serviceCamelCaseName);

            if (!serviceURL) {
                console.error(`No service URL available for ${serviceCamelCaseName}.`);
                return res.status(500).send('Service not found');
            }
            try {
                let config = {
                    method: req.method,
                    url: `${serviceURL}${req.path}`,
                    maxRedirects: 0
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

                const response = await axios(config);
                console.log(`Forwarded request to service: ${serviceName} at URL: ${serviceURL}`);

                if ([302, 307].includes(response.status)) {
                    this.incrementFailedReroutes(serviceCamelCaseName);
                    if (this.failedReroutes[serviceCamelCaseName] >= this.rerouteThreshold) {
                        const breaker = this.getCircuitBreakerForService(serviceCamelCaseName);
                        breaker.open();
                        this.failedReroutes[serviceCamelCaseName] = 0; // Reset counter after tripping the circuit
                    }
                    return res.status(response.status).send('Redirecting...');
                }

                return res.status(response.status).send(response.data);
            } catch (error) {
                if (error.response) {
                    if ([302, 307].includes(error.response.status)) {
                        this.incrementFailedReroutes(serviceCamelCaseName);
                        if (this.failedReroutes[serviceCamelCaseName] >= this.rerouteThreshold) {
                            const breaker = this.getCircuitBreakerForService(serviceCamelCaseName);
                            breaker.open();
                            this.failedReroutes[serviceCamelCaseName] = 0; // Reset counter after tripping the circuit
                        }
                    } else {
                        console.error('Server responded with error:', error.response.data);
                        return res.status(error.response.status).send(error.response.data);
                    }
                } else if (error.request) {
                    console.error('No response received:', error.request);
                } else {
                    console.error('Error calling service:', error.message);
                }
                return res.status(500).send(error.message);
            }
        });

        this.app.get('/reset-breakers', (req, res) => {
            Object.values(this.circuitBreakers).forEach(breaker => breaker.close());
            res.status(200).send("All circuit breakers have been reset");
        });
    }

    start() {
        this.app.listen(this.config.PORT, () => {
            console.log(`API Gateway is running on port ${this.config.PORT} in ${this.configManager.environment} environment`);
        });
    }
}

module.exports = ApiGateway;
