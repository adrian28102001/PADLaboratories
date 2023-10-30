const redis = require('redis');

class RedisClient {
    constructor(config, retryDuration) {
        this.config = config;
        this.retryDuration = retryDuration;
        this.connect();
    }

    connect() {
        this.redisClient = redis.createClient(this.config);
        this.redisClient.on('error', (err) => {
            console.error('Failed to connect to Redis:', err);
            setTimeout(() => this.connect(), this.retryDuration);
        });
        this.redisClient.on('connect', () => {
            console.log('Connected to Redis!');
        });
    }

    getClient() {
        return this.redisClient;
    }
}

module.exports = RedisClient;
