module.exports = {
    development: {
        PORT: 3000,
        SERVICE_DISCOVERY_URL: 'http://localhost:4000',
        CRITICAL_LOAD_THRESHOLD: 60,
        TIMEOUT_LIMIT: 5000,
        FAILURE_THRESHOLD: 3,
        FALLBACK_MESSAGE: 'Fallback: Service temporarily unavailable',
        CACHE_TTL: 30 * 60, // Cache TTL in seconds, e.g., 30 minutes
        REROUTE_THRESHOLD: 5,
        REDIS_CONFIG: {
            host: 'localhost',
            port: 6379,
        },
        CACHE_CONFIG: {
            debug: true,
            readOnError: false,
            clearOnStale: true,
            maxAge: 15 * 60 * 1000,
        },
    },
    docker: {
        PORT: 3000,
        SERVICE_DISCOVERY_URL: 'http://service-discovery:4000',
        CRITICAL_LOAD_THRESHOLD: 60,
        TIMEOUT_LIMIT: 5000,
        FAILURE_THRESHOLD: 3,
        FALLBACK_MESSAGE: 'Fallback: Service temporarily unavailable',
        CACHE_TTL: 30 * 60, // Cache TTL in seconds, e.g., 30 minutes
        REROUTE_THRESHOLD: 5,
        REDIS_CONFIG: {
            host: 'redis',
            port: 6379,
        },
        CACHE_CONFIG: {
            debug: true,
            readOnError: false,
            clearOnStale: true,
            maxAge: 15 * 60 * 1000,
        }
    }
};
