const axios = require('axios');
const https = require('https');
const { SERVICE_DISCOVERY_URL } = require('./config');
const CircuitBreaker = require('./circuitBreaker');
const config = require("./config");

const httpsAgent = new https.Agent({ rejectUnauthorized: false });

const circuitBreaker = new CircuitBreaker(
    config.TIMEOUT_LIMIT,
    config.FAILURE_THRESHOLD,
    () => {
        console.warn(config.FALLBACK_MESSAGE);
        return config.FALLBACK_MESSAGE;
    },
);

const discoverService = async (serviceName) => {
    try {
        const { data } = await circuitBreaker.call(() => axios.get(`${SERVICE_DISCOVERY_URL}/discover/${serviceName}`, {
            httpsAgent,
        }));

        if (!data) throw new Error('Service not found');
        return data;
    } catch (error) {
        console.error('Circuit breaker tripped:', error.message);
        return null;
    }
};

module.exports = discoverService;
