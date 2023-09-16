process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';
const express = require('express');
const axios = require('axios');
const redis = require('redis');
const { setupCache } = require('axios-cache-adapter');

const app = express();
const PORT = 3000;
const SERVICE_DISCOVERY_URL = 'http://localhost:4000';

const redisClient = redis.createClient({
    host: 'localhost', // Redis server address
    port: 6379         // default Redis port
});

// Create `axios` instance with pre-configured `axios-cache-adapter` using a Redis cache
const cache = setupCache({
    debug: true,
    readOnError: false,
    clearOnStale: true,
    redis: redisClient
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

app.use('/applications', async (req, res) => {
    const applicationServiceURL = await discoverService('ApplicationManagementService');
    if (!applicationServiceURL) return res.status(500).send('Service not found');

    try {
        const response = await api({
            method: req.method,
            url: `${applicationServiceURL}${req.path}`,
            data: req.body
        });

        res.send(response.data);
    } catch (error) {
        res.status(500).send(error.message);
    }
});

app.use('/joboffers', async (req, res) => {
    const jobServiceURL = await discoverService('JobManagementService');
    if (!jobServiceURL) return res.status(500).send('Service not found');

    try {
        const response = await api({
            method: req.method,
            url: `${jobServiceURL}${req.path}`,
            data: req.body
        });

        res.send(response.data);
    } catch (error) {
        res.status(500).send(error.message);
    }
});

app.listen(PORT, () => {
    console.log(`API Gateway is running on port ${PORT}`);
});