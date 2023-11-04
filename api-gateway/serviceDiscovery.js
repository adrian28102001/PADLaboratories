const axios = require('axios');
const { promisify } = require('util');
const CircuitBreaker = require('opossum');

const environment = process.env.NODE_ENV || 'development';
const config = require('./config')[environment];

// Setup circuit breaker options
const options = {
    timeout: config.TIMEOUT_LIMIT, // If our function takes longer than this, trigger a failure
    errorThresholdPercentage: config.FAILURE_THRESHOLD, // % of errors to trip the breaker
    resetTimeout: 30000 // After this time try again
};

const breaker = new CircuitBreaker(axios, options);

breaker.fallback(() => config.FALLBACK_MESSAGE);
breaker.on('open', () => console.warn('Circuit has been opened! Fallback route triggered.'));

const discoverService = async (serviceName, redisClient) => {
    const getAsync = promisify(redisClient.get).bind(redisClient);
    const setAsync = promisify(redisClient.set).bind(redisClient);

    const serviceUrlsCacheKey = `service_urls_${serviceName}`;
    try {
        const cachedUrls = await getAsync(serviceUrlsCacheKey);
        if (cachedUrls) {
            return JSON.parse(cachedUrls);
        }

        const response = await breaker.fire({
            method: 'get',
            url: `${config.SERVICE_DISCOVERY_URL}/discover/${serviceName}`
        });

        const { data } = response;

        if (!data || data.length === 0) {
            throw new Error('Service not found');
        }

        await setAsync(serviceUrlsCacheKey, JSON.stringify(data), 'EX', config.CACHE_TTL);
        return data;
    } catch (error) {
        console.error('Error in service discovery:', error.message);
        return null;
    }``
};

module.exports = discoverService;
