const redis = require('redis');

class RedisClient {
    constructor(config) {
        this.client = redis.createClient(config);

        this.client.on('error', (err) => {
            console.error("Error connecting to redis", err);
        });

        this.client.on('connect', () => {
            console.log('Connected to Redis');
        });

        this.client.on('ready', () => {
            console.log('Redis client is ready for commands');
        });
    }

    getClient() {
        return this.client;
    }
}

module.exports = RedisClient;
