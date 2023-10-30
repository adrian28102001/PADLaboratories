class Config {
    constructor(environment) {
        this.environment = environment || 'development';
        this.settings = require('./config')[this.environment];

        if (!this.settings) {
            throw new Error(`No configuration found for environment: ${this.environment}`);
        }
    }

    get(key) {
        return this.settings[key];
    }
}

module.exports = Config;
