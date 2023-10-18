const express = require('express');
const axios = require('axios');
const {setupCache} = require('axios-cache-adapter');
const redis = require('redis');
const multer = require('multer');
const FormData = require('form-data');
const config = require('./config');
const monitorLoad = require('./loadMonitor');
const discoverService = require('./serviceDiscovery');
const {SERVICE_DISCOVERY_URL} = require("./config");

const app = express();
const upload = multer();

const retryDuration = 5000; // 5 seconds, adjust as needed

app.use(upload.any());
app.use(monitorLoad);
app.use(express.json());

let serviceDiscoveryIsUp = false;

const checkServiceDiscovery = async () => {
    try {
        const response = await axios.get(`${SERVICE_DISCOVERY_URL}/health`);
        console.log("Trying to access:" + `${SERVICE_DISCOVERY_URL}/health`);

        if (response.status === 200) {
            console.log("Successfully connected to Service Discovery!");
            serviceDiscoveryIsUp = true;
            clearInterval(checkServiceDiscoveryInterval); // Clear the interval once connected
        }
    } catch (error) {
        console.log("Failed connecting to Service Discovery. Retrying...");
    }
};

const connectToRedis = () => {
    const redisClient = redis.createClient(config.REDIS_CONFIG);
    redisClient.on('error', (err) => {
        console.error('Failed to connect to Redis:', err);
        // Retry after `retryDuration`
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
    const serviceCamelCaseName = `${serviceName.charAt(0).toUpperCase()}${serviceName.slice(1)}`;
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

        // Set the adapter based on the request method
        if (req.method.toLowerCase() !== 'get') {
            config.adapter = undefined;
        } else {
            config.adapter = cache.adapter;
        }

        const {data, status} = await axios(config);
        console.log(`Forwarded request to service: ${serviceName} at URL: ${serviceURL}`);
        res.status(status).send(data);
    } catch (error) {
        console.error('Error calling service:', error);
        if (error.response) {
            return res.status(error.response.status).send(error.response.data);
        }
        res.status(500).send(error.message);
    }
});

app.listen(config.PORT, () => {
    console.log(`API Gateway is running on port ${config.PORT}`);
});

