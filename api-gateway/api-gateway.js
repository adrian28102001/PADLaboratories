const https = require('https');
const fs = require('fs');
const express = require('express');
const axios = require('axios');
const redis = require('redis');
const {setupCache} = require('axios-cache-adapter');

const app = express();
const PORT = 3000;
const SERVICE_DISCOVERY_URL = 'https://localhost:4000';
const CircuitBreaker = require('./circuitBreaker');
const httpsAgent = new https.Agent({ rejectUnauthorized: false });

const TIMEOUT_LIMIT = 5000; // Assuming 5 seconds as the task timeout limit
const circuitBreaker = new CircuitBreaker(TIMEOUT_LIMIT);
const options = {
    key: fs.readFileSync('U:\\Keys\\key.pem'),
    cert: fs.readFileSync('U:\\Keys\\cert.pem')
};

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
    httpsAgent: new https.Agent({ rejectUnauthorized: false }),
    adapter: cache.adapter
});


app.use(express.json());

const discoverService = async (serviceName) => {
    try {
        return await circuitBreaker.call(async () => {
            const response = await axios.get(`${SERVICE_DISCOVERY_URL}/discover/${serviceName}`, {
                httpsAgent: httpsAgent
            });            return response.data;
        });
    } catch (error) {
        console.error('Circuit breaker tripped:', error.message);
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

https.createServer(options, app).listen(PORT, () => {
    console.log(`API Gateway is running on port ${PORT}`);
});
