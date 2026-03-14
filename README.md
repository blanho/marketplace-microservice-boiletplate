# Marketplace

A production-grade **microservices e-commerce platform** built with .NET 9, featuring event-driven architecture, distributed tracing, and containerized deployment.

![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker)
![License](https://img.shields.io/badge/License-MIT-green)

---

## Architecture

```
                              API Gateway (YARP)
                          localhost:6100 - Rate Limiting
                                     |
          +--------------------------+-------------------------+
          |                          |                         |
          v                          v                         v
  +---------------+        +---------------+        +---------------+
  | CatalogService|        | BasketService |        | OrderService  |
  |   :6000       |        |   :6001       |        |   :6003       |
  |   Marten/PG   |        | Marten + Redis|        |   EF Core/PG  |
  +---------------+        +-------+-------+        +-------+-------+
                                   | gRPC                   ^
                                   v                        | MassTransit
                           +---------------+                | (RabbitMQ)
                           |DiscountService|   +------------+------------+
                           |  :6002 (gRPC) |   |    Message Broker       |
                           |   EF Core/PG  |   |    RabbitMQ :5672       |
                           +---------------+   +-------------------------+
```

### Tech Stack

| Layer | Technology |
|-------|------------|
| **API Gateway** | YARP Reverse Proxy, Rate Limiting |
| **Services** | ASP.NET Core Minimal APIs, Carter, MediatR |
| **Messaging** | MassTransit + RabbitMQ, Outbox Pattern |
| **Data** | PostgreSQL (Marten and EF Core), Redis |
| **Sync Communication** | gRPC (Basket to Discount) |
| **Observability** | OpenTelemetry, Jaeger, Seq |
| **Resilience** | Polly (Retry, Circuit Breaker, Timeout) |
| **Containerization** | Docker Compose |

---

## Project Structure

```
src/
├── ApiGateway/
│   └── ApiGateway/              # YARP reverse proxy
├── BuildingBlocks/
│   ├── BuildingBlocks/          # CQRS, Behaviours, Exceptions, Observability
│   ├── EventBus/                # Integration events (BasketCheckout, OrderCreated)
│   ├── Messaging/               # MassTransit extensions, Outbox pattern
│   └── SharedKernel/            # DDD base types (Entity, AggregateRoot, ValueObject)
└── Services/
    ├── BasketService/           # Shopping cart with Redis caching
    ├── CatalogService/          # Product catalog (Marten document DB)
    ├── DiscountService/         # gRPC discount/coupon service
    └── OrderService/            # Order management (Clean Architecture)
        ├── Order.API/           # Endpoints, DI
        ├── Order.Application/   # CQRS Commands/Queries, DTOs
        ├── Order.Domain/        # Aggregates, Entities, Value Objects
        └── Order.Infrastructure/# EF Core, Repository, MassTransit Consumer
```

---

## Services

| Service | Port | Database | Description |
|---------|------|----------|-------------|
| **API Gateway** | 6100 | - | YARP reverse proxy, rate limiting, health aggregation |
| **CatalogService** | 6000 | PostgreSQL (Marten) | Product catalog CRUD |
| **BasketService** | 6001 | PostgreSQL + Redis | Shopping cart, checkout publishes events |
| **DiscountService** | 6002 | PostgreSQL (EF Core) | gRPC coupon/discount service |
| **OrderService** | 6003 | PostgreSQL (EF Core) | Order management, consumes BasketCheckout events |

### Infrastructure

| Service | Port | Purpose |
|---------|------|---------|
| **PostgreSQL** | 5432-5435 | Databases (catalog, basket, discount, order) |
| **Redis** | 6379 | Basket distributed cache |
| **RabbitMQ** | 5672 / 15672 | Message broker / Management UI |
| **Jaeger** | 16686 | Distributed tracing UI |
| **Seq** | 8081 | Structured log aggregation |
| **pgAdmin** | 5050 | Database management UI |

---

## Quick Start

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### 1. Clone and Configure

```bash
git clone <repo-url>
cd marketplace
cp .env.example .env
```

### 2. Start Full Stack

```bash
docker compose up -d
```

### 3. Verify Services

| URL | Service |
|-----|---------|
| http://localhost:6100 | API Gateway |
| http://localhost:6000/swagger | Catalog API Docs |
| http://localhost:6001/swagger | Basket API Docs |
| http://localhost:6003/swagger | Order API Docs |
| http://localhost:15672 | RabbitMQ (guest/guest) |
| http://localhost:16686 | Jaeger Tracing |
| http://localhost:8081 | Seq Logs |
| http://localhost:5050 | pgAdmin |

### 4. Health Checks

```bash
# Individual service
curl http://localhost:6000/health

# Readiness (includes dependencies)
curl http://localhost:6000/health/ready
```

---

## Development

### Run Individual Services

```bash
# Start only infrastructure
docker compose up catalog.db basket.db discount.db order.db redis messagebroker -d

# Run a service locally with hot reload
cd src/Services/CatalogService/CatalogService.API
dotnet watch run
```

### Build Solution

```bash
dotnet build Marketplace.sln
```

### API Gateway Routes

| Route | Target Service |
|-------|----------------|
| `/catalog-service/**` | CatalogService :6000 |
| `/basket-service/**` | BasketService :6001 |
| `/order-service/**` | OrderService :6003 |

Example:
```bash
# Via Gateway
curl http://localhost:6100/catalog-service/products

# Direct
curl http://localhost:6000/products
```

---

## Event-Driven Flow

```
+-------------+     POST /basket/checkout     +-------------+
|BasketService| ----------------------------> |  RabbitMQ   |
+-------------+   BasketCheckoutEvent         +------+------+
                                                     |
                                                     v
                                              +-------------+
                                              |OrderService |
                                              |  Consumer   |
                                              +-------------+
                                                     |
                                                     v
                                              Creates Order
                                              in Database
```

**Integration Events:**
- `BasketCheckoutEvent` - Published when basket checkout is requested
- `OrderCreatedEvent` - Published when order is created (for future services)

---

## Observability

### Distributed Tracing (Jaeger)

All services are instrumented with OpenTelemetry. View traces at http://localhost:16686

Traces include:
- HTTP requests (ASP.NET Core)
- Database queries
- gRPC calls
- RabbitMQ message publishing/consuming

### Structured Logging (Seq)

View aggregated logs at http://localhost:8081

Filter by:
- Service name
- Correlation ID
- Log level

---

## Adding a New Feature

Each feature follows the **Vertical Slice** pattern:

```
Products/CreateProduct/
├── CreateProductCommand.cs    # ICommand<TResult> + Result record
├── CreateProductHandler.cs    # ICommandHandler implementation
├── CreateProductValidator.cs  # FluentValidation rules
└── CreateProductEndpoint.cs   # ICarterModule (HTTP route)
```

1. Create folder: `[Feature]/`
2. Add Command/Query + Result records
3. Add Handler implementing `ICommandHandler<T>` or `IQueryHandler<T>`
4. Add Validator extending `AbstractValidator<T>`
5. Add Endpoint implementing `ICarterModule`

Pipeline behaviours (validation, logging, exception handling) are automatic.

---

## Adding a New Service

1. Create project structure under `src/Services/[ServiceName]/`
2. Reference `BuildingBlocks` projects
3. Add to `Marketplace.sln`:
   ```bash
   dotnet sln add src/Services/[ServiceName]/[ServiceName].API/[ServiceName].API.csproj
   ```
4. Add to `docker-compose.yml` and `docker-compose.override.yml`
5. Add route in API Gateway `appsettings.json`

---

## Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

---

## License

MIT
