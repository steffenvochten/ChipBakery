# ChipBakery — Claude Code Guide

## Project Overview

ChipBakery is a distributed microservices bakery management system built with .NET 10 and orchestrated by .NET Aspire. Services cover orders, inventory, warehouse, suppliers, loyalty, and production.

## Technology Stack

- **Backend**: C# / .NET 10 / ASP.NET Core Minimal APIs
- **Frontend**: Blazor + Tailwind CSS
- **Orchestration**: .NET Aspire (`ChipBakery.AppHost`)
- **Database**: PostgreSQL via EF Core (one DB per service)
- **Messaging**: RabbitMQ (event-driven, async)
- **Caching**: Redis (live tracking)
- **Patterns**: Clean Architecture, DDD, Event-Driven Architecture

## Repository Structure

```
ChipBakery.AppHost/          # Aspire orchestrator — startup project
ChipBakery.ServiceDefaults/  # Shared Aspire config (telemetry, health checks)
ChipBakery.Shared/           # Shared contracts, DTOs, IEventPublisher
Web/ChipBakery.Web/          # Blazor frontend

Order/                       # Order.Service group
Inventory/                   # Inventory.Service group
Warehouse/                   # Warehouse.Service group
Supplier/                    # Supplier.Service group
Loyalty/                     # Loyalty.Service group
Production/                  # Production.Service group (background worker)
```

Each service directory contains:
```
ServiceName/
  ├── ServiceName.slnx
  ├── ServiceName.Domain/         # Entities, interfaces, exceptions — zero deps
  ├── ServiceName.Application/    # Use cases, DTOs, FluentValidation, mapping
  ├── ServiceName.Infrastructure/ # EF Core, repositories, event publisher
  └── ServiceName.Service/        # Minimal API endpoints, middleware, DI root
```

## Clean Architecture Rules

- **Domain**: No external dependencies. Pure entities, interfaces, domain events.
- **Application**: Depends only on Domain. Holds use cases, validators, DTOs.
- **Infrastructure**: Depends on Application + Domain. EF Core, repos, `IEventPublisher` impl.
- **API layer** (`*.Service`): Thin endpoints only. Maps `ChipBakery.Shared` types ↔ internal DTOs at the boundary.

See `CLEAN_ARCHITECTURE.md` for the full implementation pattern.

## Shared Contracts (`ChipBakery.Shared`)

| Type | Description |
|---|---|
| `ProductItem(Guid, string, decimal, int)` | Inventory `/available` response |
| `OrderRequest(Guid ProductId, int Quantity, string CustomerName)` | Order placement |
| `OrderResponse(bool Success, string Message, Guid? OrderId)` | Order result |
| `IEventPublisher` | Domain event publishing interface |
| `OrderStatus` enum | `Placed`, `Processing`, `Completed`, `Cancelled` — stored as string |

## Event Publishing

- All services publish via `IEventPublisher` (lives in `ChipBakery.Shared`).
- Currently uses `MockEventPublisher` (logs structured JSON to Aspire dashboard).
- RabbitMQ is already provisioned in AppHost — add `.WithReference(rabbitmq)` per service to wire it up.

## Key Workflows

- **Order validation is synchronous**: always check stock/ingredient availability before placing an order on the event bus.
- **Production.Service** is a background worker consuming `OrderPlaced` events; it tracks progress in Redis.

## Styling Conventions

- Use **Tailwind utility classes** exclusively. Avoid custom CSS unless unavoidable.
- Edit styles in `Web/ChipBakery.Web/Styles/app.css`; compiled output goes to `wwwroot/app.css` automatically.
- Design language: "Cute Bakery" — soft pastels, warm tones, `rounded-2xl`/`rounded-3xl`, subtle glassmorphism.
- Dark mode: use `dark:` prefix; theme toggled via `[data-theme]` on `<html>`.

## Adding a New Service

1. Create a root-level directory named after the service.
2. Add four projects following the Clean Architecture pattern above.
3. Use `..\..\` for references to shared root projects.
4. Create a `ServiceName.slnx` inside the directory.
5. Add projects to the main `ChipBakery.slnx` and register in `ChipBakery.AppHost`.

## Solution Files

- **`ChipBakery.slnx`** — full system, all projects; use for cross-service work.
- **`ServiceName/ServiceName.slnx`** — isolated; includes only that service + `ChipBakery.Shared` + `ChipBakery.ServiceDefaults`.

## Running Locally

Prerequisites: .NET 10 SDK, Docker Desktop, .NET Aspire workload.

```
Set ChipBakery.AppHost as startup project → F5
```

The Aspire Dashboard opens automatically with links to services and real-time logs.

## Authentication

No authentication required at this stage. Future options: Duende IdentityServer, OpenIddict, Zitadel, Authelia, or Authentik.
