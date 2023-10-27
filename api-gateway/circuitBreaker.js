class CircuitBreaker {
    constructor(timeoutLimit, failureThreshold = 3, fallbackFunction = null) {
        this.state = 'CLOSED';
        this.failureCount = 0;
        this.timeoutLimit = timeoutLimit;
        this.failureThreshold = failureThreshold;
        this.resetTimer = null;
        this.fallbackFunction = fallbackFunction;
    }

    logState() {
        console.log(`Circuit state: ${this.state}`);
    }

    async call(asyncFunc) {
        this.logState();

        if (this.state === 'OPEN') {
            console.log('Circuit is OPEN. Not making the call.');

            if (this.fallbackFunction) {
                return this.fallbackFunction();
            }

            throw new Error('Service is currently unavailable.');
        }

        try {
            const result = await asyncFunc();
            if (this.state === 'HALF_OPEN') {
                this.state = 'CLOSED';
                this.failureCount = 0;
                clearTimeout(this.resetTimer);
                console.log('Call succeeded in HALF_OPEN state. Moving to CLOSED state.');
            }
            return result;
        } catch (error) {
            this.failureCount++;
            console.error('Failure detected. Incrementing failure count:', this.failureCount);
            if (this.failureCount >= this.failureThreshold) {
                this.state = 'OPEN';
                console.error('Circuit is tripped. Moving to OPEN state.');
                this.resetTimer = setTimeout(() => {
                    this.state = 'HALF_OPEN';
                    console.log('Timeout elapsed. Moving to HALF_OPEN state.');
                }, this.timeoutLimit * 3.5);
            }
            throw error;
        }
    }
}

module.exports = CircuitBreaker;
