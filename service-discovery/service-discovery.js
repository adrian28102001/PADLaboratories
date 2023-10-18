const express = require('express');
const redis = require('redis');
const app = express();
const PORT = 4000;
const { promisify } = require('util');
const redisClient = redis.createClient({
    host: 'localhost',
    port: 6379,
    retry_strategy: function (options) {
        if (options.attempt > 10) {
            return undefined;
        }
        return 1000;
    }
});

redisClient.on('error', (err) => {
    console.error("Error connecting to redis", err);
});

redisClient.on('connect', () => {
    console.log('Connected to Redis');
});

redisClient.on('ready', () => {
    console.log('Redis client is ready for commands');
});

let services = {};

app.use(express.json());

app.get('/services', async (req, res) => {
    try {
        // Fetch all the service names (keys) from Redis
        const keysAsync = promisify(redisClient.keys).bind(redisClient);
        const serviceNames = await keysAsync('*');

        if (!serviceNames || !serviceNames.length) {
            return res.status(200).send([]);
        }

        // Fetch all services details from Redis
        const multi = redisClient.multi();
        for (let name of serviceNames) {
            multi.hgetall(name);
        }

        const execAsync = promisify(multi.exec).bind(multi);
        const servicesDetails = await execAsync();

        // Format the services for the response
        const formattedServices = serviceNames.map((name, index) => ({
            name: name,
            ...servicesDetails[index]
        }));

        // Return the formatted services
        res.status(200).send(formattedServices);
    } catch (err) {
        console.error("Error fetching services from Redis:", err);
        res.status(500).send("Internal server error.");
    }
});


app.post('/register', (req, res) => {
    let {name, url, load} = req.body;
    if (!name || !url || load == null) return res.status(400).send("Invalid registration details.");

    services[name] = {name, url, load }; // Store the service in the services object

    // Register the service in Redis with load information
    redisClient.hmset(name, 'url', url, 'load', load, (err) => {
        if (err) {
            console.error("Error setting value in Redis:", err);
            return res.status(500).send("Internal server error.");
        }
        redisClient.expire(name, 300); // Set expiry time for the service in Redis
        console.log(`Service ${name} was registered having an address ${url} with load ${load}`);
        res.send("Service registered successfully");
    });
});

app.get('/discover/:name', (req, res) => {
    let name = req.params.name.toLowerCase();

    const hgetallAsync = promisify(redisClient.hgetall).bind(redisClient);

    hgetallAsync(name).then(service => {
        if (service) {
            res.send(service.url);
        } else {
            service = services[name];
            if (!service) return res.status(404).send("Service not found.");

            // Cache the service in Redis
            redisClient.hmset(name, 'url', service.url, 'load', service.load, (err) => {
                if (err) {
                    console.error("Error caching value in Redis:", err);
                    return res.status(500).send("Internal server error.");
                }
                redisClient.expire(name, 300);
                res.send(service.url);
            });
        }
    }).catch(err => {
        console.error("Error getting value from Redis:", err);
        return res.status(500).send("Internal server error.");
    });
});

app.get('/health', (req, res) => {
    res.status(200).send('Service Discovery is healthy');
});

app.listen(PORT, () => {
    console.log(`Service Discovery is running on port ${PORT}`);
});