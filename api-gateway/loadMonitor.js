const { CRITICAL_LOAD_THRESHOLD } = require('./config');

let requestCount = 0;
let lastCheckedTimestamp = Date.now();

const monitorLoad = (req, res, next) => {
    requestCount++;
    const currentTime = Date.now();
    const elapsedTime = currentTime - lastCheckedTimestamp;

    if (elapsedTime >= 1000) {
        const rps = requestCount / (elapsedTime / 1000);
        if (rps > CRITICAL_LOAD_THRESHOLD) {
            console.warn('Critical load detected! Requests per second:', rps);
        }
        requestCount = 0;
        lastCheckedTimestamp = currentTime;
    }
    next();
};

module.exports = monitorLoad;
