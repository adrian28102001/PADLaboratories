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

const discoverService = async (serviceName, hazelcastClient) => {
    const serviceUrlsMap = await hazelcastClient.getMap('serviceUrls'); // Get distributed map for service URLs
    const serviceUrlsCacheKey = `service_urls_${serviceName}`;

    try {
        const cachedUrls = await serviceUrlsMap.get(serviceUrlsCacheKey);
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

        // Cache the URLs in Hazelcast for future requests
        await serviceUrlsMap.set(serviceUrlsCacheKey, JSON.stringify(data));

        return data; // data should be an array of URLs
    } catch (error) {
        console.error('Error in service discovery:', error.message);
        return null; // Return null to signify the failure in discovery
    }
};

module.exports = discoverService;
