class CircuitBreaker {
    constructor(timeoutLimit) {
        this.state = 'CLOSED';
        this.failureCount = 0;
        this.timeoutLimit = timeoutLimit;
        this.resetTimer = null;
    }

    async call(asyncFunc) {
        if (this.state === 'OPEN') {
            console.log('Circuit is OPEN. Not making the call.');
            throw new Error('Service is currently unavailable.');
        }

        try {
            const result = await asyncFunc();
            if (this.state === 'HALF_OPEN') {
                this.state = 'CLOSED';
                this.failureCount = 0;
                clearTimeout(this.resetTimer);
            }
            return result;
        } catch (error) {
            this.failureCount++;
            if (this.failureCount >= 3) {
                this.state = 'OPEN';
                console.error('Circuit is tripped. Moving to OPEN state.');
                this.resetTimer = setTimeout(() => {
                    this.state = 'HALF_OPEN';
                }, this.timeoutLimit * 3.5);
            }
            throw error;
        }
    }
}
