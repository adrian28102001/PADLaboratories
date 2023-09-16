process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

const express = require('express');
const axios = require('axios');
const redis = require('redis');
const {setupCache} = require('axios-cache-adapter');

const app = express();
const PORT = 3000;
const SERVICE_DISCOVERY_URL = 'http://localhost:4000';

const redisClient = redis.createClient({
    host: 'localhost',
    port: 6379
});

// Configure caching for axios
const cache = setupCache({
    debug: true,
    readOnError: false,
    clearOnStale: true,
    maxAge: 15 * 60 * 1000,  // cache for 15 minutes
    redis: redisClient,
    exclude: {
        query: false
    }
});

const api = axios.create({
    adapter: cache.adapter
});

app.use(express.json());

const discoverService = async (serviceName) => {
    try {
        const response = await axios.get(`${SERVICE_DISCOVERY_URL}/discover/${serviceName}`);
        return response.data;
    } catch (error) {
        return null;
    }
};

app.use('/', async (req, res) => {
    const serviceName = req.path.split('/')[1];
    const serviceCamelCaseName = `${serviceName.charAt(0)}${serviceName.slice(1)}`;
    const serviceURL = await discoverService(serviceCamelCaseName);

    if (!serviceURL) return res.status(500).send('Service not found');

    try {
        const response = await api({
            method: req.method,
            url: `${serviceURL}${req.path}`,
            data: req.body,
        });

        res.send(response.data);
    } catch (error) {
        res.status(500).send(error.message);
    }
});

app.get('/health', (req, res) => {
    res.status(200).send('API Gateway is healthy');
});

app.listen(PORT, () => {
    console.log(`API Gateway is running on port ${PORT}`);
});
