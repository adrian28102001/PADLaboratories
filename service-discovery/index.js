process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';
const express = require('express');
const app = express();
const PORT = 4000;

let services = {};  // To store registered services

app.use(express.json());

app.post('/register', (req, res) => {
    let {name, url} = req.body;
    if (!name || !url) return res.status(400).send("Invalid registration details.");

    // Register the service
    services[name] = url;
    console.log(`Service ${name} was registered having an address ${url}`);
    res.send("Service registered successfully");
});

app.get('/discover/:name', (req, res) => {
    let name = req.params.name;
    let service = services[name];
    if (!service) return res.status(404).send("Service not found.");
    res.send(service);
});

app.get('/health', (req, res) => {
    res.status(200).send('Service Discovery is healthy');
});

app.listen(PORT, () => {
    console.log(`Service Discovery is running on port ${PORT}`);
});
