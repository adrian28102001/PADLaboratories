const express = require('express');
const https = require('https');
const axios = require('axios');
const { setupCache } = require('axios-cache-adapter');
const redis = require('redis');
const multer = require('multer');
const FormData = require('form-data');
const config = require('./config');
const monitorLoad = require('./loadMonitor');
const discoverService = require('./serviceDiscovery');

const app = express();
const upload = multer();
const redisClient = redis.createClient(config.REDIS_CONFIG);
const cache = setupCache({ ...config.CACHE_CONFIG, redis: redisClient });

const httpsAgent = new https.Agent({ rejectUnauthorized: false });

app.use(upload.any());
app.use(monitorLoad);
app.use(express.json());

app.get('/health', (req, res) => {
    res.status(200).send('API Gateway is healthy');
});

app.use('/', async (req, res) => {
    const serviceName = req.path.split('/')[1];
    const serviceCamelCaseName = `${serviceName.charAt(0).toUpperCase()}${serviceName.slice(1)}`;
    const serviceURL = await discoverService(serviceCamelCaseName);

    if (!serviceURL) return res.status(500).send('Service not found');

    try {
        let config = {
            method: req.method,
            url: `${serviceURL}${req.path}`,
            httpsAgent,
        };

        if (req.is('multipart/form-data')) {
            const formData = new FormData();
            req.files.forEach(file => formData.append(file.fieldname, file.buffer, {
                filename: file.originalname,
                contentType: file.mimetype,
            }));
            Object.keys(req.body).forEach(key => formData.append(key, req.body[key]));
            config.data = formData;
        } else {
            config.data = req.body;
        }

        // Set the adapter based on the request method
        if (req.method.toLowerCase() !== 'get') {
            config.adapter = undefined;
        } else {
            config.adapter = cache.adapter;
        }

        const { data, status } = await axios(config);
        res.status(status).send(data);
    } catch (error) {
        console.error('Error calling service:', error);
        if (error.response) {
            return res.status(error.response.status).send(error.response.data);
        }
        res.status(500).send(error.message);
    }
});

https.createServer(config.SSL_OPTIONS, app).listen(config.PORT, () => {
    console.log(`API Gateway is running on port ${config.PORT}`);
});
