const express = require('express');
const RedisClient = require('./redisClient');
const ServiceRegistry = require('./serviceRegistry');
const config = require('./appConfig')

class App {
    constructor() {
        this.app = express();
        this.config = config[process.env.NODE_ENV || 'development'];
        this.redisClient = new RedisClient(this.config.REDIS_CONFIG).getClient();
        this.serviceRegistry = new ServiceRegistry(this.redisClient);
        this.setupMiddlewares();
        this.setupRoutes();
    }

    setupMiddlewares() {
        this.app.use(express.json());
    }

    setupRoutes() {
        this.app.get('/services', this.handleGetServices.bind(this));
        this.app.post('/register', this.handleRegisterService.bind(this));
        this.app.get('/discover/:name', this.handleDiscoverService.bind(this));
        this.app.get('/health', this.handleHealthCheck.bind(this));
    }

    async handleGetServices(req, res) {
        try {
            const services = await this.serviceRegistry.getAllServices();
            res.status(200).send(services);
        } catch (err) {
            console.error("Error fetching services from Redis:", err);
            res.status(500).send("Internal server error.");
        }
    }

    async handleRegisterService(req, res) {
        try {
            const { name, url, load } = req.body;

            if (!name || !url || load == null) {
                return res.status(400).send("Invalid registration details.");
            }

            const message = await this.serviceRegistry.registerService(name, url, load);
            res.send(message);
        } catch (err) {
            console.error("Error registering service:", err);
            res.status(500).send("Internal server error.");
        }
    }

    async handleDiscoverService(req, res) {
        try {
            const name = req.params.name.toLowerCase();
            const url = await this.serviceRegistry.discoverService(name);

            if (url) {
                res.send(url);
            } else {
                res.status(404).send("Service not found.");
            }
        } catch (err) {
            console.error("Error discovering service:", err);
            res.status(500).send("Internal server error.");
        }
    }

    handleHealthCheck(req, res) {
        res.status(200).send('Service Discovery is healthy');
    }

    start() {
        const PORT = this.config.PORT || 4000;
        this.app.listen(PORT, () => {
            console.log(`Service Discovery is running on port ${PORT} and environment: ${this.config.environment}`);
        });
    }
}

module.exports = App;
