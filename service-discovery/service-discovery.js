const express = require('express');
const redis = require('redis');
const app = express();
const { promisify } = require('util');

const environment = process.env.NODE_ENV || 'development';
const config = require('./config')[environment];

if (!config) {
    console.error(`No configuration found for environment: ${environment}`);
    process.exit(1);
}

const redisClient = redis.createClient(config.REDIS_CONFIG);

redisClient.on('error', (err) => {
    console.error("Error connecting to redis", err);
});

redisClient.on('connect', () => {
    console.log('Connected to Redis');
});

redisClient.on('ready', () => {
    console.log('Redis client is ready for commands');
});

app.use(express.json());

app.get('/services', async (req, res) => {
    try {
        const keysAsync = promisify(redisClient.keys).bind(redisClient);
        const serviceNames = await keysAsync('*');

        if (!serviceNames || !serviceNames.length) {
            return res.status(200).send([]);
        }

        const multi = redisClient.multi();
        for (let name of serviceNames) {
            multi.hgetall(name);
        }

        const execAsync = promisify(multi.exec).bind(multi);
        const servicesDetails = await execAsync();

        const formattedServices = serviceNames.map((name, index) => ({
            name: name,
            ...servicesDetails[index],
            urls: JSON.parse(servicesDetails[index].urls)
        }));

        res.status(200).send(formattedServices);
    } catch (err) {
        console.error("Error fetching services from Redis:", err);
        res.status(500).send("Internal server error.");
    }
});

app.post('/register', (req, res) => {
    let { name, urls, load } = req.body;
    if (!name || !urls || !Array.isArray(urls) || urls.length === 0 || load == null) {
        return res.status(400).send("Invalid registration details.");
    }

    const urlsJson = JSON.stringify(urls);
    redisClient.hmset(name, 'urls', urlsJson, 'load', load, 'currentIndex', 0, (err) => {
        if (err) {
            console.error("Error setting value in Redis:", err);
            return res.status(500).send("Internal server error.");
        }
        redisClient.expire(name, config.REDIS_EXPIRY); // Use expiry time from config
        console.log(`Service ${name} was registered with URLs ${urls} and load ${load}`);
        res.send("Service registered successfully");
    });
});

app.get('/discover/:name', async (req, res) => {
    const name = req.params.name.toLowerCase();
    const hgetallAsync = promisify(redisClient.hgetall).bind(redisClient);

    try {
        const service = await hgetallAsync(name);
        if (service && service.urls) {
            const urls = JSON.parse(service.urls);
            const currentIndex = parseInt(service.currentIndex, 10);
            const nextIndex = (currentIndex + 1) % urls.length;
            redisClient.hset(name, 'currentIndex', nextIndex);
            res.send(urls[currentIndex]);
        } else {
            return res.status(404).send("Service not found.");
        }
    } catch (err) {
        console.error("Error getting value from Redis:", err);
        return res.status(500).send("Internal server error.");
    }
});

app.get('/health', (req, res) => {
    res.status(200).send('Service Discovery is healthy');
});

const PORT = config.PORT || 4000;
app.listen(PORT, () => {
    console.log(`Service Discovery is running on port ${PORT} and environment: ${environment}`);
});