const axios = require('axios');
const { promisify } = require('util');
const CircuitBreaker = require('./circuitBreaker');
const environment = process.env.NODE_ENV || 'development';
const config = require('./config')[environment];

const circuitBreaker = new CircuitBreaker(
    config.TIMEOUT_LIMIT,
    config.FAILURE_THRESHOLD,
    () => {
        console.warn(config.FALLBACK_MESSAGE);
        return config.FALLBACK_MESSAGE;
    },
);

const discoverService = async (serviceName, redisClient) => {
    const getAsync = promisify(redisClient.get).bind(redisClient);
    const setAsync = promisify(redisClient.set).bind(redisClient);

    const serviceUrlsCacheKey = `service_urls_${serviceName}`;
    try {
        // First, try to get the URLs from Redis cache
        const cachedUrls = await getAsync(serviceUrlsCacheKey);
        if (cachedUrls) {
            return JSON.parse(cachedUrls);
        }

        // No URLs in cache, so use circuit breaker to call service discovery
        const { data } = await circuitBreaker.call(() =>
            axios.get(`${config.SERVICE_DISCOVERY_URL}/discover/${serviceName}`)
        );

        if (!data || data.length === 0) {
            throw new Error('Service not found');
        }

        // Cache the URLs in Redis for future requests
        await setAsync(serviceUrlsCacheKey, JSON.stringify(data), 'EX', config.CACHE_TTL);

        return data; // data should be an array of URLs
    } catch (error) {
        console.error('Error in service discovery:', error.message);
        return null; // Return null to signify the failure in discovery
    }
};

module.exports = discoverService;
