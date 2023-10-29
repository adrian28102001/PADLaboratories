const express = require('express');
const axios = require('axios');
const {setupCache} = require('axios-cache-adapter');
const redis = require('redis');
const multer = require('multer');
const FormData = require('form-data');
const environment = process.env.NODE_ENV || 'development';
const config = require('./config')[environment];

if (!config) {
    console.error(`No configuration found for environment: ${environment}`);
    process.exit(1);
}

console.log(`Running in ${environment} environment`);

const monitorLoad = require('./loadMonitor');
const discoverService = require('./serviceDiscovery');

const app = express();
const upload = multer();

const retryDuration = 5000;

app.use(upload.any());
app.use(monitorLoad);
app.use(express.json());

let serviceDiscoveryIsUp = false;

const checkServiceDiscovery = async () => {
    try {
        const response = await axios.get(`${config.SERVICE_DISCOVERY_URL}/health`);
        console.log("Trying to access:" + `${config.SERVICE_DISCOVERY_URL}/health`);

        if (response.status === 200) {
            console.log("Successfully connected to Service Discovery!");
            serviceDiscoveryIsUp = true;
            clearInterval(checkServiceDiscoveryInterval);
        }
    } catch (error) {
        console.log("Failed connecting to Service Discovery. Retrying...");
    }
};

const connectToRedis = () => {
    const redisClient = redis.createClient(config.REDIS_CONFIG);
    redisClient.on('error', (err) => {
        console.error('Failed to connect to Redis:', err);
        setTimeout(connectToRedis, retryDuration);
    });
    redisClient.on('connect', () => {
        console.log('Connected to Redis!');
    });
    return redisClient;
};

const redisClient = connectToRedis();
const cache = setupCache({...config.CACHE_CONFIG, redis: redisClient});

const checkServiceDiscoveryInterval = setInterval(checkServiceDiscovery, 10000); // 10 seconds

app.get('/health', (req, res) => {
    if (serviceDiscoveryIsUp) {
        res.status(200).send('API Gateway is healthy');
    } else {
        res.status(503).send('API Gateway is not ready yet. Service Discovery not available.');
    }
});

app.use('/', async (req, res) => {
    const serviceName = req.path.split('/')[1];
    const serviceCamelCaseName = `${serviceName.charAt(0).toLowerCase()}${serviceName.slice(1)}`;
    const serviceURL = await discoverService(serviceCamelCaseName);

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
            config.adapter = cache.adapter;
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

app.listen(config.PORT, () => {
    console.log(`API Gateway is running on port ${config.PORT} in ${environment} environment`);
});

