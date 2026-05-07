# ChipBakery Architecture & Conventions

This file documents the architectural standards and development conventions for the ChipBakery project.

## Repository Structure

The project follows a grouped service structure to keep the root clean and enable isolated development.

### Shared Projects (Root)
- `ChipBakery.AppHost`: The .NET Aspire orchestrator.
- `ChipBakery.ServiceDefaults`: Shared Aspire configurations (telemetry, health checks).
- `ChipBakery.Shared`: Shared contracts, DTOs, and common utilities.

### Service Grouping
Each service is contained within its own root-level directory. Complex services following Clean Architecture should include all layers within this directory.

Example structure for a service:
```text
ServiceName/
  ├── ServiceName.slnx           # Isolated solution file
  ├── ServiceName.Domain/        # Domain layer (Entities, Exceptions, Interfaces)
  ├── ServiceName.Application/   # Application layer (Services, DTOs, Validators)
  ├── ServiceName.Infrastructure/# Infrastructure layer (Persistence, Clients, Events)
  └── ServiceName.Service/       # API/Entry point layer
```

### Solution Files
- **Main Solution (`ChipBakery.slnx`)**: Includes every project in the repository. Use this for full-system builds and cross-service refactoring.
- **Service Solutions (`ServiceName/ServiceName.slnx`)**: Includes only the projects for that specific service plus `ChipBakery.Shared` and `ChipBakery.ServiceDefaults`. Use these for isolated development and debugging on smaller machines.

## New Service Creation
When adding a new service:
1. Create a new directory in the root named after the service.
2. Group all related projects within that directory.
3. Update project references to point to root-level shared projects using `..\..\`.
4. Create a service-specific `.slnx` file.
5. Add the new projects to the main `ChipBakery.slnx` and `ChipBakery.AppHost`.

## Technology Stack
- **Framework**: .NET 10
- **Orchestration**: .NET Aspire
- **Architecture**: Clean Architecture / DDD
- **Messaging**: RabbitMQ (Event-Driven)
- **Database**: PostgreSQL (EF Core)
- **Frontend**: Blazor with Tailwind CSS

## Styling Conventions
- **Tailwind CSS**: Use Tailwind utility classes for all styling. Avoid custom vanilla CSS unless absolutely necessary.
- **Source CSS**: Edit `Web/ChipBakery.Web/Styles/app.css`. The compiled output is automatically generated to `wwwroot/app.css` during the build process.
- **Design System**: Follow the "Cute Bakery" aesthetic — soft pastels, warm tones, rounded corners (`rounded-2xl`, `rounded-3xl`), and minimal glassmorphism effects.
- **Dark Mode**: Support dark mode using the `dark:` prefix in Tailwind classes. The theme is managed via a `[data-theme]` attribute on the `<html>` element.
* **Inventory.Service**: Manages finished goods (items for sale) that were produced earlier. ✅ Refactored to Clean Architecture.
* **Warehouse.Service**: Manages raw materials (flour, yeast, etc.) that are supplied by the supplier. ✅ Refactored to Clean Architecture.
* **Supplier.Service**: Acts as a mock service that handles incoming ingredient transports into the ecosystem. ✅ Refactored to Clean Architecture.
* **Order.Service**: Handles transactions. It must synchronously check if there are enough items/ingredients available before accepting an order. ✅ Refactored to Clean Architecture.
* **Production.Service**: Background worker consuming `OrderPlaced` events to handle baking schedules and updates tracking in Redis. ✅ Refactored to Clean Architecture.
* **Loyalty.Service**: Handles loyalty/rewards logic. ✅ Refactored to Clean Architecture.
* **Agents.Service**: Autonomous agent simulation (Clients, Suppliers, Bakers). Uses **Ollama** for LLM-based reasoning and **SignalR** to stream activity to the frontend. ⚠️ **Note**: This service is a single project and does not follow the 4-layer Clean Architecture pattern.

## Service Architecture Pattern
All services should be built using Clean Architecture with 4 separate projects per service:
- `[Service].Domain` — Pure entities, interfaces, events, exceptions. Zero dependencies.
- `[Service].Application` — Use cases/service layer, FluentValidation validators, DTOs, mapping. Depends only on Domain.
- `[Service].Infrastructure` — EF Core DbContext, repositories, event publisher. Depends on Application + Domain.
- `[Service].API` (named `[Service].Service` to preserve Aspire AppHost references) — Thin Minimal API endpoints, exception handler middleware, DI composition root.

**Exception**: `Agents.Service` is exempt from this 4-layer requirement as it functions primarily as a background worker and LLM client without persistent domain entities.

See: `CLEAN_ARCHITECTURE.md` in this repository for the full implementation pattern and template.

## Synchronous vs Asynchronous Workflows
* **Order Validation**: Orders must **always** perform a synchronous check to ensure enough stock/ingredients are available before the order is finalized and placed on the event bus.

## Event Publishing
* All services publish domain events via `IEventPublisher`.
* Currently mocked by `MockEventPublisher` (logs as structured JSON — visible in Aspire dashboard).
* **To wire to RabbitMQ**: See the replacement guide comments in `Inventory.Infrastructure/Events/MockEventPublisher.cs`.
* AppHost already provisions RabbitMQ (`rabbitmq`) — services just need `.WithReference(rabbitmq)` added.

## Orchestration & Containers
* **.NET Aspire**: Manages service discovery and life cycle.
* **Ollama**: Hosted as a sidecar container in AppHost to provide local LLM capabilities (Llama 3.2) for the `Agents.Service`.

## Shared Contracts (ChipBakery.Shared)
* `ProductItem(Guid Id, string Name, decimal Price, int AvailableQuantity)` — returned by Inventory `/available` endpoint.
* `OrderRequest(Guid ProductId, int Quantity, string CustomerName)` — used by Order.Service and the `/deduct` endpoint.
* `OrderResponse(bool Success, string Message, Guid? OrderId)` — returned by Order.Service.
* `IEventPublisher` — **lives in `ChipBakery.Shared`** (moved from Inventory.Domain to avoid per-service duplication). Domain projects reference ChipBakery.Shared.
* `OrderStatus` enum — `Placed`, `Processing`, `Completed`, `Cancelled`. Stored as string in DB.
* The API boundary (endpoints) is the **only** place that maps between `ChipBakery.Shared` types and internal Application DTOs.

## Identity & Authentication
* **Current State**: No authentication required at this stage.
* **Future Alternatives to Keycloak**: If authentication is added later, potential lightweight alternatives include:
  * **Duende IdentityServer** / **OpenIddict** (Native .NET OAuth/OIDC solutions).
  * **Zitadel** (Go-based, cloud-native, excellent API and multi-tenancy).
  * **Authelia** or **Authentik** (Self-hosted, easy to configure).
