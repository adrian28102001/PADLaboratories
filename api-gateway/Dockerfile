# Use a Node.js base image
FROM node:16

# Set the working directory
WORKDIR /usr/src/app

# Copy package.json and package-lock.json
COPY package*.json ./

# Install dependencies
RUN npm install

# Copy the rest of the code
COPY . .

# Expose port (assuming 3000 or change as per your config)
EXPOSE 3000

# Start the app
CMD ["node", "api-gateway.js"]
