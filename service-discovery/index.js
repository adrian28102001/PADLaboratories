const express = require('express');
const redis = require('redis');
const app = express();
const PORT = 4000;

const redisClient = redis.createClient({
    host: 'localhost',
    port: 6379
});

let services = {};

app.use(express.json());

app.post('/register', (req, res) => {
    let {name, url} = req.body;
    if (!name || !url) return res.status(400).send("Invalid registration details.");

    // Register the service in memory and in Redis
    services[name] = url;
    redisClient.set(name, url, 'EX', 300); // cache for 5 minutes

    console.log(`Service ${name} was registered having an address ${url}`);
    res.send("Service registered successfully");
});

app.get('/discover/:name', (req, res) => {
    let name = req.params.name;

    // First try to get the service from Redis
    redisClient.get(name, (err, service) => {
        if (service) {
            res.send(service);
        } else {
            service = services[name];
            if (!service) return res.status(404).send("Service not found.");

            // Cache the service in Redis
            redisClient.set(name, service, 'EX', 300); // cache for 5 minutes

            res.send(service);
        }
    });
});

app.get('/health', (req, res) => {
    res.status(200).send('Service Discovery is healthy');
});

app.listen(PORT, () => {
    console.log(`Service Discovery is running on port ${PORT}`);
});