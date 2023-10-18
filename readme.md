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

**API Gateway:** A centralized entry point for all incoming requests. It abstracts the internal service structure from the
user, ensuring flexibility and scalability. The gateway is also responsible for caching responses and implementing
resilience patterns like Circuit Breakers.

**Microservices:** Independent units of deployment, each with its dedicated database. They register themselves with the
Service Discovery and handle the actual business logic for the user's request.

##  Concurrent Tasks Limit:
To ensure smooth performance and responsiveness, we've implemented a concurrent tasks limit mechanism. This helps to
prevent system overloads, ensuring that only a specified number of simultaneous requests are processed. Any excess
concurrent requests are queued or responded to with a suitable message, ensuring system stability.

##  Implementation Details

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

**Circuit Breaker:** To prevent system failures from cascading and giving systems a chance to recover, we've implemented a
Circuit Breaker pattern. If a service call fails three times consecutively within a specific window (TIMEOUT_LIMIT *
3.5), the gateway trips the circuit and stops making calls to the service for a set duration.

### 3. Service Discovery:

**Redis-backed Directory:** The service discovery utilizes Redis as its data store. Services register their names and
URLs with the discovery, which retains them for a 5-minute period (configurable). Post expiration, services need to
re-register.

**In-memory Fallback:** For added reliability, there's also an in-memory record of services. If the Service Discovery fails
to find the service in Redis (maybe due to data expiration), it falls back to this in-memory record.

### 4. Load Balancing:


While the provided code snippets do not detail the load balancing mechanism, our architecture implements a Round
Robin strategy for load balancing, allowing equal distribution of requests across 3-4 replicas of each service.

### 5. Health Monitoring:


Each critical component, i.e., the API Gateway and Service Discovery, has a health endpoint (/health). This endpoint
can be pinged to check the health status of the services. If any service gets pinged beyond a certain limit (e.g., 60
pings per second), it can be set to raise an alert, indicating potential problems or overloads.

**Endpoints:**

**ApplicationManagementService**

GET /applicationmanagement/api/applications: Retrieves a list of all applications.
GET /applicationmanagement/api/applications/{id}: Retrieves a specific application by its ID.
GET /applicationmanagement/api/applications/job/{jobId}: Retrieves an application by a specific job ID.
POST /applicationmanagement/api/applications: Adds a new job application.
PUT /applicationmanagement/api/applications/{id}: Updates a specific application.
DELETE /applicationmanagement/api/applications/{id}: Deletes a specific application.

**JobManagementService**

GET /jobmanagement/api/jobs/applications/{jobId}: Retrieves a job application for a specific job ID via the API Gateway.
GET /jobmanagement/api/joboffers/{id}: Retrieves a specific job offer by its ID.

https://documenter.getpostman.com/view/18378097/2s9YCARAm5
Docker link: docker.io/adriangherman2001/lab1-api-gateway