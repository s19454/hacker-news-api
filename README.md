## README

### Overview

* Minimal API that returns top Hacker News stories
* BackgroundService polls Hacker News API, fetches best story IDs, hydrates stories, sorts by score, caches top N
* HTTP endpoint exposes `GET /hackernews/stories/best/{n}` returning cached results
* Cache is in-memory, thread-safe, refreshed on interval

### Running the application

* Requires .NET 7 SDK installed
* Navigate to `src/HackerNewsApi.Host`
* Run `dotnet run`
* Service starts IIS and background refresh loop
* Default endpoint: `GET http://localhost:{port}/hackernews/stories/best/10`
* Requires config keys present in appsettings:
  * `Cache:MaxSize` → max number of cached stories
  * `Cache:RefreshMiliseconds` → background refresh interval
 
<img width="1877" height="2043" alt="image" src="https://github.com/user-attachments/assets/2ae273dc-347e-4cab-9336-5b7e7e728c02" />

### Assumptions

* Hacker News API is stable and returns valid JSON for story and ID endpoints
* Network latency/temporary errors are tolerated by retry loops
* Cache refresh duration is shorter than consumer API latency expectations
* In-memory cache is acceptable (no persistence, no scale-out)
* First request may block until cache warm-up
* All failures in background refresh only log error, no fallback strategy
* API tightly coupled to external API response structure

### Enhancements (given more time)

#### Architecture

* Replace simple cache with MemoryCache + TTL + eviction
* Add Polly resilience policies (retry, circuit breaker) for Hacker News API calls
* Add typed HttpClient with proper timeouts and handler pipeline
* Add DTOs/transformers to decouple domain from HN API format
* Replace BackgroundService with IHostedService + channels for more control

#### Performance

* Replace per-ID `GetStoryAsync` with batching or parallel throttling
* Replace `Task.WhenAll` with controlled parallelism to avoid exhausting sockets
* Add metrics (Prometheus/Grafana) for cache refresh timing, failures, throughput

#### API Improvements

* Add OpenAPI/Swagger definition
* Add `GET /health` and `GET /metrics`
* Add filtering/sorting options (e.g., by time, author)
* Move count argument to query

#### Caching Strategy

* Cache stories separately from ids
* Support distributed caching (Redis) for multi-node deployments
* Add incremental updates rather than full refresh each cycle
* Add warm-up routine on startup so cache is not empty for first few seconds

#### Testing

* Add full integration tests using TestServer
* Add contract tests for Hacker News API assumptions
* Add load tests verifying cache timing & concurrency correctness

#### DevOps

* Dockerfile + docker-compose
* CI pipeline with linting, analyzers, tests
* Add environment-based config layering (dev/staging/production)

