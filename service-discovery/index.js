const App = require('./app');

try {
    const app = new App();
    app.start();
} catch (error) {
    console.error('Failed to start the application:', error.message);
    process.exit(1);
}