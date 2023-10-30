const { promisify } = require('util');
const EXPIRY_TIME = 86400; // 24 hours in seconds

class ServiceRegistry {
    constructor(redisClient) {
        this.redisClient = redisClient;
    }

    async registerService(name, url, load) {
        name = name.toLowerCase();

        const service = await this.getService(name);

        let urls = [];
        let currentIndex = 0;
        if (service) {
            urls = JSON.parse(service.urls);
            currentIndex = parseInt(service.currentIndex, 10);
        }

        if (urls.includes(url)) {
            console.log(`Service ${name} with URL ${url} is already registered.`);
            return "Service already registered.";
        }

        urls.push(url);

        const updatedService = {
            urls: JSON.stringify(urls),
            load: load.toString(),
            currentIndex: currentIndex.toString()
        };

        await this.setService(name, updatedService);
        console.log(`Service ${name} was registered with URL ${url} and load ${load}`);
        return "Service registered successfully";
    }

    async getService(name) {
        const hgetallAsync = promisify(this.redisClient.hgetall).bind(this.redisClient);
        return hgetallAsync(name.toLowerCase());
    }

    async setService(name, serviceData) {
        const hmsetAsync = promisify(this.redisClient.hmset).bind(this.redisClient);
        await hmsetAsync(name.toLowerCase(), serviceData);
        this.redisClient.expire(name.toLowerCase(), EXPIRY_TIME);
    }

    async discoverService(name) {
        const service = await this.getService(name);

        if (service && service.urls) {
            const urls = JSON.parse(service.urls);
            const currentIndex = parseInt(service.currentIndex, 10);
            const nextIndex = (currentIndex + 1) % urls.length;

            await this.setService(name, { ...service, currentIndex: nextIndex.toString() });
            return urls[nextIndex];
        }

        return null;
    }

    async getAllServices() {
        const keysAsync = promisify(this.redisClient.keys).bind(this.redisClient);
        const serviceNames = await keysAsync('*');

        if (!serviceNames || !serviceNames.length) {
            return [];
        }

        const multi = this.redisClient.multi();
        for (let name of serviceNames) {
            multi.hgetall(name);
        }

        const execAsync = promisify(multi.exec).bind(multi);
        const servicesDetails = await execAsync();

        return serviceNames.map((name, index) => ({
            name,
            ...servicesDetails[index],
            urls: JSON.parse(servicesDetails[index].urls)
        }));
    }
}

module.exports = ServiceRegistry;
