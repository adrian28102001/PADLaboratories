class CircuitBreaker {
    constructor(timeoutLimit, failureThreshold = 3, rerouteThreshold = 5, fallbackFunction = null) {
        this.state = 'CLOSED';
        this.failureCount = 0;
        this.rerouteCount = 0;
        this.timeoutLimit = timeoutLimit;
        this.failureThreshold = failureThreshold;
        this.rerouteThreshold = rerouteThreshold;
        this.resetTimer = null;
        this.fallbackFunction = fallbackFunction;
    }

    logState() {
        console.log(`Circuit state: ${this.state}`);
    }

    tripCircuit() {
        this.state = 'OPEN';
        console.error('Circuit is tripped. Moving to OPEN state.');
        this.resetTimer = setTimeout(() => {
            this.state = 'HALF_OPEN';
            console.log('Timeout elapsed. Moving to HALF_OPEN state.');
        }, this.timeoutLimit * 3.5);
    }

    async call(asyncFunc, isReroute = false) {
        this.logState();

        if (this.state === 'OPEN') {
            console.log('Circuit is OPEN. Not making the call.');

            if (this.fallbackFunction) {
                return this.fallbackFunction();
            }

            throw new Error('Service is currently unavailable.');
        }

        if (isReroute) {
            this.rerouteCount++;
            if (this.rerouteCount > this.rerouteThreshold) {
                this.tripCircuit();
                throw new Error('Circuit tripped due to excessive reroutes.');
            }
        }

        try {
            const result = await asyncFunc();
            this.successfulRequest();
            return result;
        } catch (error) {
            this.failedRequest();
            throw error;
        }
    }

    successfulRequest() {
        if (this.state === 'HALF_OPEN') {
            this.state = 'CLOSED';
            this.failureCount = 0;
            this.rerouteCount = 0;
            clearTimeout(this.resetTimer);
            console.log('Call succeeded in HALF_OPEN state. Moving to CLOSED state.');
        }
    }

    failedRequest() {
        this.failureCount++;
        console.error('Failure detected. Incrementing failure count:', this.failureCount);
        if (this.failureCount >= this.failureThreshold) {
            this.tripCircuit();
        }
    }
}

module.exports = CircuitBreaker;
