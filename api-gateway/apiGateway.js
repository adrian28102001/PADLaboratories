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

class ApiGateway {
    constructor() {
        this.configManager = new ConfigManager(process.env.NODE_ENV);
        this.config = this.configManager.getConfig();
        this.redisClient = new RedisClient(this.config.REDIS_CONFIG, 5000).getClient();
        this.cache = setupCache({ ...this.config.CACHE_CONFIG, redis: this.redisClient });
        this.serviceDiscoveryChecker = new ServiceDiscoveryChecker(this.config.SERVICE_DISCOVERY_URL, 10000);
        this.app = express();
        this.upload = multer();
        this.setupMiddleware();
        this.setupRoutes();
    }

    setupMiddleware() {
        this.app.use(this.upload.any());
        this.app.use(loadMonitor);
        this.app.use(express.json());
    }

    setupRoutes() {
        this.app.get('/health', (req, res) => {
            if (this.serviceDiscoveryChecker.getStatus()) {
                res.status(200).send('API Gateway is healthy');
            } else {
                res.status(503).send('API Gateway is not ready yet. Service Discovery not available.');
            }
        });

        this.app.use('/', async (req, res) => {
            const serviceName = req.path.split('/')[1];
            const serviceCamelCaseName = `${serviceName.charAt(0).toLowerCase()}${serviceName.slice(1)}`;
            const serviceURL = await serviceDiscovery(serviceCamelCaseName);

            if (!serviceURL) return res.status(500).send('Service not found');

            try {
                let config = {
                    method: req.method,
                    url: `${serviceURL}${req.path}`,
                };

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

                const {data, status} = await axios(config);
                console.log(`Forwarded request to service: ${serviceName} at URL: ${serviceURL}`);
                res.status(status).send(data);
            } catch (error) {
                if (error.response) {
                    console.error('Server responded with error:', error.response.data);
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
