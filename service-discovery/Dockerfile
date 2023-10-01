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

# Expose port (assuming 4000 or change as per your script)
EXPOSE 4000

# Start the app
CMD ["node", "service-discovery.js"]
