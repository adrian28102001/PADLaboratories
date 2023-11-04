class ServiceRegistry {
    constructor() {
        this.services = {}; // In-memory storage for services
    }
    async registerService(name, url) {
        name = name.toLowerCase();

        if (!this.services[name]) {
            this.services[name] = [];
        }

        if (!this.services[name].includes(url)) {
            this.services[name].push(url);
            console.log(`Service ${name} was registered with URL ${url}`);
            return "Service registered successfully.";
        } else {
            console.log(`Service ${name} with URL ${url} is already registered.`);
            return "Service already registered.";
        }
    }

    async getService(name) {
        name = name.toLowerCase();
        return this.services[name] || null;
    }

    async discoverService(name) {
        const serviceUrls = await this.getService(name);
        if (serviceUrls) {
            return serviceUrls; // Return the list of URLs
        }
        return null;
    }

    async getAllServices() {
        let allServices = [];
        for (const [name, urls] of Object.entries(this.services)) {
            allServices.push({
                name,
                urls
            });
        }
        return allServices;
    }
}

module.exports = ServiceRegistry;