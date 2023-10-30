class ConfigManager {
    constructor(environment) {
        this.environment = environment || 'development';
        this.config = require('./config')[this.environment];

        if (!this.config) {
            console.error(`No configuration found for environment: ${this.environment}`);
            process.exit(1);
        }

        console.log(`Running in ${this.environment} environment`);
    }

    getConfig() {
        return this.config;
    }
}

module.exports = ConfigManager;
