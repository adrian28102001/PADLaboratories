const axios = require('axios');

class ServiceDiscoveryChecker {
    constructor(url, intervalDuration) {
        this.url = url;
        this.intervalDuration = intervalDuration;
        this.isUp = false;
        this.startChecking();
    }

    async check() {
        try {
            const response = await axios.get(`${this.url}/health`);
            console.log("Trying to access:", this.url);

            if (response.status === 200) {
                console.log("Successfully connected to Service Discovery!");
                this.isUp = true;
                clearInterval(this.interval);
            }
        } catch (error) {
            console.log("Failed connecting to Service Discovery. Retrying...");
        }
    }

    startChecking() {
        this.interval = setInterval(() => this.check(), this.intervalDuration);
    }

    getStatus() {
        return this.isUp;
    }
}

module.exports = ServiceDiscoveryChecker;
