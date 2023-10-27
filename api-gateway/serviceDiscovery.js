const axios = require('axios');
const {SERVICE_DISCOVERY_URL} = require('./configs/config');
const CircuitBreaker = require('./circuitBreaker');
const config = require("./configs/config");

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
        const {data} = await circuitBreaker.call(() => axios.get(`${SERVICE_DISCOVERY_URL}/discover/${serviceName}`));

        if (!data) throw new Error('Service not found');
        return data;
    } catch (error) {
        console.error('Circuit breaker tripped:', error.message);
        return null;
    }
};

module.exports = discoverService;
