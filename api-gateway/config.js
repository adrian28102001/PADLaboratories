module.exports = {
    PORT: 3000,
    SERVICE_DISCOVERY_URL: 'http://localhost:4000',
    CRITICAL_LOAD_THRESHOLD: 60,
    TIMEOUT_LIMIT: 5000,
    FAILURE_THRESHOLD: 3,
    FALLBACK_MESSAGE: 'Fallback: Service temporarily unavailable',
    REDIS_CONFIG: {
        host: 'localhost',
        port: 6379,
    },
    CACHE_CONFIG: {
        debug: true,
        readOnError: false,
        clearOnStale: true,
        maxAge: 15 * 60 * 1000,
    }
};
