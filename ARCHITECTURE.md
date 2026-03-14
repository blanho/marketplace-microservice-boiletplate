# Marketplace — System Architecture

> Production-grade .NET 9 microservices platform.

---

## High-Level Architecture

```
                          ┌─────────────────────┐
                          │     API Gateway      │
                          │       (YARP)         │
                          │   localhost:6100      │
                          └────┬───┬───┬───┬────┘
                               │   │   │   │
              ┌────────────────┘   │   │   └────────────────┐
              ▼                    ▼   ▼                    ▼
     ┌────────────────┐  ┌──────────────────┐  ┌────────────────────┐
     │ CatalogService │  │  BasketService   │  │    OrderService    │
     │  :6000 (HTTP)  │  │  :6001 (HTTP)    │  │    :6003 (HTTP)    │
     └───────┬────────┘  └───┬─────────┬────┘  └──┬────────────┬───┘
             │               │         │           │            │
             ▼               ▼         ▼           ▼            │
     ┌──────────────┐ ┌──────────┐ ┌────────┐ ┌──────────┐     │
     │ catalog_db   │ │basket_db │ │ Redis  │ │ order_db │     │
     │ (Postgres)   │ │(Postgres)│ │ Cache  │ │(Postgres)│     │
     └──────────────┘ └──────────┘ └────────┘ └──────────┘     │
                                                                │
     ┌────────────────────────────────────────────────────┐     │
     │                  RabbitMQ                          │◄────┘
     │          (MassTransit Event Bus)                   │
     │  BasketCheckout → Order saga orchestrator          │
     └────────────────────────────────────────────────────┘
                          │
              ┌───────────┴───────────┐
              ▼                       ▼
     ┌────────────────┐     ┌────────────────┐
     │DiscountService │     │    Jaeger       │
     │  :5002 (gRPC)  │     │  (Tracing UI)  │
     └───────┬────────┘     │  :16686        │
             ▼              └────────────────┘
     ┌──────────────┐
     │ discount_db  │
     │ (Postgres)   │
     └──────────────┘
```

---

## Communication Patterns

| From → To                  | Protocol | Pattern           | Description                                      |
|----------------------------|----------|-------------------|--------------------------------------------------|
| Client → API Gateway       | HTTP     | Reverse Proxy     | YARP routes to downstream services               |
| Basket → Discount          | gRPC     | Sync Request      | Price lookup during basket update                |
| Basket → Order             | RabbitMQ | Async Event       | `BasketCheckoutEvent` triggers order creation    |
| Order (internal)           | RabbitMQ | Saga Orchestrator | Multi-step order workflow with compensation      |
| All Services → Jaeger      | OTLP     | Trace Export       | Distributed tracing via OpenTelemetry            |
| All Services → Seq         | HTTP     | Log Export         | Structured logging via Serilog + Seq             |

---

## Service Responsibilities

### CatalogService (`:6000`)
- **Owns**: Product aggregate (name, category, description, image, price)
- **Storage**: Marten (PostgreSQL document DB)
- **Pattern**: Vertical slice (command/query per feature folder)
- **Publishes**: `ProductPriceChangedEvent` (future)

### BasketService (`:6001`)
- **Owns**: ShoppingCart aggregate
- **Storage**: Marten (PostgreSQL) + Redis (distributed cache)
- **Consumes (gRPC)**: DiscountService for coupon lookup
- **Publishes**: `BasketCheckoutEvent` → triggers order workflow

### DiscountService (`:5002`)
- **Owns**: Coupon entity
- **Storage**: EF Core (PostgreSQL)
- **Exposes**: gRPC service for discount lookups
- **Pattern**: Repository pattern with EF Core

### OrderService (`:6003`)
- **Owns**: Order aggregate (order lines, shipping, payment status)
- **Storage**: EF Core (PostgreSQL) + Outbox table
- **Consumes**: `BasketCheckoutEvent` from RabbitMQ
- **Pattern**: Clean Architecture (Domain → Application → Infrastructure → API)
- **Saga**: Order processing workflow with compensation

### API Gateway (`:6100`)
- **Role**: Single entry point, reverse proxy
- **Tech**: YARP (Yet Another Reverse Proxy)
- **Features**: Rate limiting, health aggregation, request routing

---

## Folder Structure

```
marketplace/
├── docker-compose.yml                    # Infrastructure + services
├── docker-compose.override.yml           # Dev-only port mappings, tooling
├── Marketplace.sln
├── ARCHITECTURE.md                       # This file
│
├── src/
│   ├── ApiGateway/
│   │   └── ApiGateway/
│   │       ├── Program.cs                # YARP config, rate limiting, health
│   │       ├── ApiGateway.csproj
│   │       ├── appsettings.json          # YARP route/cluster config
│   │       └── Properties/
│   │
│   ├── BuildingBlocks/
│   │   ├── BuildingBlocks/               # CQRS, Behaviours, Exceptions
│   │   ├── SharedKernel/                 # Entity, AggregateRoot, ValueObject
│   │   ├── EventBus/                     # IEventBus, IntegrationEvent (abstractions)
│   │   └── Messaging/                    # MassTransit + RabbitMQ implementation
│   │       ├── Messaging.csproj
│   │       ├── Extensions/               # DI extensions for MassTransit
│   │       └── Outbox/                   # Outbox pattern with EF Core
│   │
│   └── Services/
│       ├── CatalogService/
│       │   └── CatalogService.API/
│       │       ├── Program.cs
│       │       ├── Products/             # Vertical slices (CreateProduct, GetProducts, etc.)
│       │       ├── Entities/
│       │       └── Exceptions/
│       │
│       ├── BasketService/
│       │   └── BasketService.API/
│       │       ├── Program.cs
│       │       ├── Basket/               # Feature slices (GetBasket, StoreBasket, etc.)
│       │       ├── Data/                 # Repository + Cache decorator
│       │       ├── Entities/
│       │       └── Protos/
│       │
│       ├── DiscountService/
│       │   └── Discount.Grpc/
│       │       ├── Program.cs
│       │       ├── Services/             # gRPC service implementations
│       │       ├── Data/                 # EF Core context + repository
│       │       ├── Models/
│       │       └── Protos/
│       │
│       └── OrderService/
│           ├── Order.API/                # Minimal API endpoints, DI composition root
│           ├── Order.Application/        # MediatR handlers, DTOs, integration events
│           ├── Order.Domain/             # Aggregates, entities, value objects, domain events
│           └── Order.Infrastructure/     # EF Core, outbox, MassTransit consumers
```

---

## Cross-Cutting Concerns

### Observability (OpenTelemetry)
```
All services → OpenTelemetry Collector → Jaeger (traces) + Seq (logs)
```
- **Traces**: HTTP requests, gRPC calls, RabbitMQ messages, DB queries
- **Metrics**: Request duration, error rates, queue depth
- **Logs**: Structured logging via Serilog → Seq

### Resilience (Polly)
- **HTTP Clients**: Retry (exponential backoff) + Circuit Breaker + Timeout
- **gRPC Clients**: Retry with hedging for idempotent calls
- **Database**: EF Core `EnableRetryOnFailure` for transient faults

### Health Checks
- `/health` — liveness (service is running)
- `/health/ready` — readiness (dependencies are available)
- Checked dependencies: PostgreSQL, Redis, RabbitMQ

### Outbox Pattern
- Integration events stored in `OutboxMessages` table within the same DB transaction
- Background worker (`OutboxProcessor`) publishes to RabbitMQ, marks as processed
- Guarantees at-least-once delivery without distributed transactions

---

## Order Saga Workflow

```
BasketCheckout event received
        │
        ▼
  ┌─────────────┐     ┌──────────────────┐     ┌─────────────────┐
  │ CreateOrder  │────▶│ ValidatePayment  │────▶│ CompleteOrder    │
  │   (step 1)  │     │    (step 2)      │     │   (step 3)      │
  └──────┬──────┘     └────────┬─────────┘     └─────────────────┘
         │ fail                │ fail
         ▼                     ▼
  ┌─────────────┐     ┌──────────────────┐
  │ CancelOrder │     │  RefundPayment   │
  │(compensate) │     │  (compensate)    │
  └─────────────┘     └──────────────────┘
```

---

## Docker Compose Services

| Service          | Image                    | Port(s)        | Purpose                    |
|------------------|--------------------------|----------------|----------------------------|
| catalog.db       | postgres:17-alpine       | 5432           | CatalogService database    |
| basket.db        | postgres:17-alpine       | 5433           | BasketService database     |
| discount.db      | postgres:17-alpine       | 5434           | DiscountService database   |
| order.db         | postgres:17-alpine       | 5435           | OrderService database      |
| basket.cache     | redis:7-alpine           | 6379           | Basket distributed cache   |
| messagebroker    | rabbitmq:4-management    | 5672 / 15672   | Event bus (AMQP + UI)      |
| jaeger           | jaegertracing/all-in-one | 16686 / 4317   | Distributed tracing UI     |
| seq              | datalust/seq             | 8081           | Structured log aggregator  |
| pgadmin          | dpage/pgadmin4:8         | 5050           | Database management UI     |

---

## Production Best Practices

1. **No secrets in config** — use environment variables, Docker secrets, or Azure Key Vault
2. **Idempotent consumers** — all message handlers check for duplicate processing
3. **Correlation IDs** — propagated through HTTP headers, message properties, and trace context
4. **Graceful shutdown** — `IHostedService.StopAsync` with `CancellationToken` drains in-flight work
5. **Database migrations** — EF Core migrations run at startup only in development; use CI/CD in production
6. **Container health checks** — every Docker service has `healthcheck` + `depends_on.condition`
7. **Rate limiting** — API Gateway uses ASP.NET Core rate limiting middleware
8. **Structured logging** — Serilog with enrichers (CorrelationId, MachineName, Environment)
9. **Circuit breaker** — Polly policies prevent cascade failures across service boundaries
10. **Outbox pattern** — guarantees at-least-once event delivery without 2PC
