module.exports = {
    development: {
        PORT: 4000,
        REDIS_CONFIG: {
            host: 'localhost',
            port: 6379,
            retry_strategy: function (options) {
                if (options.attempt > 10) {
                    return undefined;
                }
                return 1000;
            }
        },
        REDIS_EXPIRY: 86400,
    },
    docker: {
        PORT: 4000,
        REDIS_CONFIG: {
            host: 'redis',
            port: 6379,
            retry_strategy: function (options) {
                if (options.attempt > 10) {
                    return undefined;
                }
                return 1000;
            }
        },
        REDIS_EXPIRY: 86400,
    }
};
