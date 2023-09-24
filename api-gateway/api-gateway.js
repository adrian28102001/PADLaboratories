const express = require('express');
const https = require('https');
const fs = require('fs');
const axios = require('axios');
const CircuitBreaker = require('./circuitBreaker');
const { setupCache } = require('axios-cache-adapter');
const redis = require('redis');

const PORT = 3000;
const SERVICE_DISCOVERY_URL = 'https://localhost:4000';
const CRITICAL_LOAD_THRESHOLD = 60; // RPS (Requests Per Second)
const TIMEOUT_LIMIT = 5000; // 5 seconds

const app = express();
const httpsAgent = new https.Agent({ rejectUnauthorized: false });
const circuitBreaker = new CircuitBreaker(TIMEOUT_LIMIT);
const redisClient = redis.createClient({
    host: 'localhost',
    port: 6379
});

const cache = setupCache({
    debug: true,
    readOnError: false,
    clearOnStale: true,
    maxAge: 15 * 60 * 1000, // Cache for 15 minutes
    redis: redisClient,
    exclude: {
        query: false
    }
});

const api = axios.create({
    httpsAgent,
    adapter: cache.adapter
});

let requestCount = 0;
let lastCheckedTimestamp = Date.now();

const monitorLoad = (req, res, next) => {
    requestCount++;
    const currentTime = Date.now();
    const elapsedTime = currentTime - lastCheckedTimestamp;

    if (elapsedTime >= 1000) {
        const rps = requestCount / (elapsedTime / 1000);
        if (rps > CRITICAL_LOAD_THRESHOLD) {
            console.warn('Critical load detected! Requests per second:', rps);
        }
        requestCount = 0;
        lastCheckedTimestamp = currentTime;
    }
    next();
};

const discoverService = async (serviceName) => {
    try {
        const { data } = await circuitBreaker.call(() => axios.get(`${SERVICE_DISCOVERY_URL}/discover/${serviceName}`, {
            httpsAgent
        }));
        return data;
    } catch (error) {
        console.error('Circuit breaker tripped:', error.message);
        return null;
    }
};

app.use(express.json());
app.use(monitorLoad);

app.use('/', async (req, res) => {
    const serviceName = req.path.split('/')[1];
    const serviceCamelCaseName = `${serviceName.charAt(0).toUpperCase()}${serviceName.slice(1)}`;
    const serviceURL = await discoverService(serviceCamelCaseName);

    if (!serviceURL) return res.status(500).send('Service not found');

    try {
        const { data } = await api({
            method: req.method,
            url: `${serviceURL}${req.path}`,
            data: req.body,
        });
        res.send(data);
    } catch (error) {
        res.status(500).send(error.message);
    }
});

app.get('/health', (req, res) => {
    res.status(200).send('API Gateway is healthy');
});

const options = {
    key: fs.readFileSync('U:\\Keys\\key.pem'),
    cert: fs.readFileSync('U:\\Keys\\cert.pem')
};

https.createServer(options, app).listen(PORT, () => {
    console.log(`API Gateway is running on port ${PORT}`);
});