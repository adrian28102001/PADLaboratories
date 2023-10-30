## Architecture Overview

In today's digital age, the job recruitment process has been largely moved online, giving rise to the necessity for
systems that can efficiently manage job applications and offers. The employment application platform we're considering
fits perfectly into a microservices architecture due to its inherently modular nature. The separate components such as
managing job applications, overseeing job offers, and potentially even handling interviews and feedback, all represent
individual domains that would benefit from being managed independently. By implementing our system as a distributed
service, we not only ensure scalability and fault tolerance but also parallel development and deployment without causing
systemic disruptions.

Consider Facebook, for instance. As a platform, it's not just about social networking anymore. It has evolved into a
multi-faceted ecosystem handling marketplace listings, advertisements, games, and more. To manage this complexity and
scale, Facebook employs a microservices-based approach, ensuring that each of its components can evolve without
affecting the others. Similarly, as our employment application platform grows, adding more features or scaling specific
components becomes more feasible and less risky.

**User Request Flow:** When a user sends a request, it first reaches the API Gateway. The API Gateway is responsible for
directing the request to the appropriate service. To achieve this, the gateway contacts the Service Discovery to find
out which service can fulfill the requested endpoint. Once the relevant service is identified, the API Gateway proxies
the request to it. Responses follow the reverse path: from the microservice back to the user, through the API Gateway.

**Service Discovery:** Acts like a directory for all active services. Services register themselves with their respective
URLs. This allows the API Gateway to quickly look up where to send the incoming requests.

**API Gateway:** A centralized entry point for all incoming requests. It abstracts the internal service structure from
the
user, ensuring flexibility and scalability. The gateway is also responsible for caching responses and implementing
resilience patterns like Circuit Breakers.

**Microservices:** Independent units of deployment, each with its dedicated database. They register themselves with the
Service Discovery and handle the actual business logic for the user's request.

## Concurrent Tasks Limit:

To ensure smooth performance and responsiveness, we've implemented a concurrent tasks limit mechanism. This helps to
prevent system overloads, ensuring that only a specified number of simultaneous requests are processed. Any excess
concurrent requests are queued or responded to with a suitable message, ensuring system stability.

## Implementation Details

### 1. Services/API Communication:

**Two Microservices:** We've implemented two services that communicate with each other.
These services are responsible for the actual business logic and data manipulation.

**Each with its Database:** Every service is backed by its database, ensuring data encapsulation
and reducing tight coupling between services.

### 2. API Gateway:

**Proxying Requests:** The gateway decodes the user's request path to determine the target service. It then asks the
Service Discovery for the URL of the targeted service and proxies the request to it.

**Caching with Redis:** To reduce redundant calls and improve performance, we implemented caching using Redis. Any data
fetched via the gateway is cached for 15 minutes using axios-cache-adapter. This ensures quicker subsequent access.

**Circuit Breaker:** To prevent system failures from cascading and giving systems a chance to recover, we've implemented
a
Circuit Breaker pattern. If a service call fails three times consecutively within a specific window (TIMEOUT_LIMIT *
3.5), the gateway trips the circuit and stops making calls to the service for a set duration.

### 3. Service Discovery:

**Redis-backed Directory:** The service discovery utilizes Redis as its data store. Services register their names and
URLs with the discovery, which retains them for a 5-minute period (configurable). Post expiration, services need to
re-register.

**In-memory Fallback:** For added reliability, there's also an in-memory record of services. If the Service Discovery
fails
to find the service in Redis (maybe due to data expiration), it falls back to this in-memory record.

### 4. Load Balancing:

While the provided code snippets do not detail the load balancing mechanism, our architecture implements a Round
Robin strategy for load balancing, allowing equal distribution of requests across 3-4 replicas of each service.

### 5. Health Monitoring:

Each critical component, i.e., the API Gateway and Service Discovery, has a health endpoint (/health). This endpoint
can be pinged to check the health status of the services. If any service gets pinged beyond a certain limit (e.g., 60
pings per second), it can be set to raise an alert, indicating potential problems or overloads.

### Endpoints

https://documenter.getpostman.com/view/18378097/2s9YCARAm5

To import collection just open the link and in the top corner press Run in Postman,
it will open the app or website for you, with the imported collection

### Docker:

https://hub.docker.com/repository/docker/adriangherman2001/pad/general

Step-by-Step Instructions:
Pull the Docker Images:
Before you can run the images, you need to pull them to your local machine. Pull each image from the
adriangherman2001/pad repository using the following commands:

```=dockerfile
docker pull adriangherman2001/pad:postgres-latest
docker pull adriangherman2001/pad:redis-latest
docker pull adriangherman2001/pad:applicationmanagem...
docker pull adriangherman2001/pad:jobmanagementservi...
docker pull adriangherman2001/pad:lab1-servicediscover...
```

Run the Docker Images:
After pulling the images, you can run them using the docker run command. For instance, to run the postgres-latest image,
you can use:

```=dockerfile
docker run --name some-postgres -d adriangherman2001/pad:postgres-latest
```

Adjust the --name flag value (e.g., some-postgres) as you see fit for each image.

Repeat the above docker run command for each of the pulled images, adjusting the tag name and container name
accordingly.

# Part2

### What does "multiple reroutes" mean?

In software, "reroute" means to redirect a request to a different service or instance when the original one fails. For
instance, if you have multiple copies of a service (for load balancing), and one fails, you might reroute the request to
another copy.

### How to trip Circuit Breaker if multiple reroutes happen?

Monitoring: The circuit breaker constantly monitors all requests.

Thresholds: Decide on a threshold for reroutes. For example, "If 5 requests are rerouted in 10 seconds".

Tripping: Once that threshold is exceeded, the circuit breaker trips. This means it will stop any further requests to
the failing service for a predefined "reset time".

Reset: After the reset time, the circuit breaker allows requests to go through again and checks if the service is
healthy. If it's still failing, it trips again.

Implementing Circuit Breaker:
There are libraries and tools which provide out-of-the-box circuit breaker functionality. One of the most popular is
Hystrix by Netflix.

### How to Achieve Service High Availability?

Redundancy: Just like having multiple ice cream shop branches, have multiple instances (or copies) of your service
running. If one instance fails, others can take over.

Load Balancing: This is like having a person who directs customers to different branches based on which shop is less
crowded. In tech, a load balancer distributes incoming requests to multiple service instances to ensure no single
instance is overwhelmed.

Regular Health Checks: The manager checks each shop branch regularly to ensure they're open and serving customers.
Similarly, perform regular checks on service instances to detect and fix issues before they become critical.

Data Backup & Recovery: Always have a backup of your data. If there's a data loss in one instance, you can recover it
from backups.

### What is the ELK Stack?

ELK stands for Elasticsearch, Logstash, and Kibana.

Elasticsearch: A search engine that stores logs.
Logstash: Collects and sends logs to Elasticsearch.
Kibana: A web interface to view and search through the logs stored in Elasticsearch.
Think of it like a library:

Elasticsearch is the collection of books.
Logstash is the librarian who categorizes and shelves books.
Kibana is the catalog system to find and read the books.

### Steps to Implement 2-Phase Commit:

1. Preparation Phase:
   Each microservice/database is asked to "prepare" to commit or abort the transaction but not to commit it yet.
   Each microservice/database locks the necessary resources and confirms if it can go ahead with the commit.
2. Commit Phase:
   If all microservices/databases confirm they can commit, the main service tells them all to commit.
   If any of them say they can't commit, the main service tells them all to abort.
   Steps to Implement Consistent Hashing:
1. Hash Function:
   You'll need a good hash function. This function will take the input (like a cache key) and return a position on our
   imaginary ring.

2. Distributing Nodes:
   Using the same hash function, decide the position for each cache node on the ring.

3. Storing and Retrieving:
   To store an item, hash its key to get a position on the ring, then move clockwise to find the nearest node. This node
   is where you'll store the item. For retrieval, do the same thing: hash, find the position, and get the item from the
   nearest node.

4. Handling Node Changes:
   If a node is added or removed, only the items nearest to that node are affected. This minimizes the items that need
   to be moved or rehashed.

### Steps to Implement Consistent Hashing:

1. Hash Function:
   You'll need a good hash function. This function will take the input (like a cache key) and return a position on our
   imaginary ring.

2. Distributing Nodes:
   Using the same hash function, decide the position for each cache node on the ring.

3. Storing and Retrieving:
   To store an item, hash its key to get a position on the ring, then move clockwise to find the nearest node. This node
   is where you'll store the item. For retrieval, do the same thing: hash, find the position, and get the item from the
   nearest node.

4. Handling Node Changes:
   If a node is added or removed, only the items nearest to that node are affected. This minimizes the items that need
   to be moved or rehashed.

### Cache High Availability (HA):

It ensures that a cache system remains operational even in the face of failures. This means if one cache server fails,
there are backup servers that can provide the cached data without disruption.

### Why is Cache HA important?

Uptime: Your application doesn't crash or slow down significantly if a cache node fails.
Data Integrity: Data in the cache remains consistent and reliable.
How to Achieve Cache High Availability:

1. Replication:
   Have multiple copies of your cache. When one cache node fails, requests are automatically routed to a backup node.

2. Clustering:
   Group multiple cache servers together. If one fails, another one in the cluster takes over.

3. Automatic Failover:
   If a cache node fails, the system automatically detects this and reroutes requests to a healthy node.

### Long-running saga transactions

A saga is a sequence of transactions that can update multiple business entities potentially across multiple
microservices. Each transaction in a saga updates the business entity and publishes a message or event to trigger the
next transaction.

If one of the transactions fails, the saga executes a series of compensating transactions to undo the changes made by
the preceding transactions.