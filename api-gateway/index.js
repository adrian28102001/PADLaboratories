process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';
const express = require('express');
const axios = require('axios');
const app = express();
const PORT = 3000;
const SERVICE_DISCOVERY_URL = 'http://localhost:4000'; // assuming this is where the service discovery is running

app.use(express.json());

// Helper function to discover service
const discoverService = async (serviceName) => {
    try {
        const response = await axios.get(`${SERVICE_DISCOVERY_URL}/discover/${serviceName}`);
        return response.data;
    } catch (error) {
        return null;
    }
}

// Route to ApplicationManagementService
app.use('/applications', async (req, res) => {
    const applicationServiceURL = await discoverService('ApplicationManagementService');
    if (!applicationServiceURL) return res.status(500).send('Service not found');

    try {
        const response = await axios({
            method: req.method,
            url: `${applicationServiceURL}${req.path}`,
            data: req.body
        });

        res.send(response.data);
    } catch (error) {
        res.status(500).send(error.message);
    }
});

// Route to JobManagementService
app.use('/joboffers', async (req, res) => {
    const jobServiceURL = await discoverService('JobManagementService');
    if (!jobServiceURL) return res.status(500).send('Service not found');

    try {
        const response = await axios({
            method: req.method,
            url: `${jobServiceURL}${req.path}`,
            data: req.body
        });

        res.send(response.data);
    } catch (error) {
        res.status(500).send(error.message);
    }
});

app.get('/health', (req, res) => {
    res.status(200).send('API Gateway is healthy');
});

app.listen(PORT, () => {
    console.log(`API Gateway is running on port ${PORT}`);
});
