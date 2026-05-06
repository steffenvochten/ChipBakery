# ChipBakery 🥐

ChipBakery is a modern, distributed microservices application built with **.NET 10** and orchestrated using **.NET Aspire**. It demonstrates a scalable architecture for a bakery management system, handling everything from order placement to inventory tracking and production scheduling.

## 🏗️ Architecture Overview

The project follows a microservices pattern, with **.NET Aspire** acting as the central orchestrator (`AppHost`) to manage service discovery, connection strings, and infrastructure dependencies.

### Core Services
All backend services strictly follow the Clean Architecture pattern, divided into Domain, Application, Infrastructure, and API layers.
- **ChipBakery.Web:** A Blazor-based frontend for users to browse and place orders.
- **Order.Service:** Manages customer orders and publishes events when orders are placed.
- **Inventory.Service:** Tracks stock levels for ingredients and finished goods.
- **Warehouse.Service:** Handles storage locations and stock movements.
- **Supplier.Service:** Manages external vendors and ingredient procurement.
- **Loyalty.Service:** Tracks customer rewards and bakery points.
- **Production.Service:** A background worker that consumes order events and schedules production.

### Infrastructure (Managed by Aspire)
- **PostgreSQL:** Distributed databases for each microservice.
- **Redis:** Used for live tracking and high-speed caching.
- **RabbitMQ:** The message broker facilitating asynchronous communication (e.g., "OrderPlaced" events).
- **pgAdmin / Redis Commander:** UI tools for monitoring data during development.

## 🚀 Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for containers)
- [.NET Aspire Workload](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/setup)

### Running the Application
1. Clone the repository.
2. Ensure Docker is running.
3. Open the solution in **JetBrains Rider** or **Visual Studio 2022**.
4. Set `ChipBakery.AppHost` as the startup project.
5. Press `F5` to launch.

Once started, the **Aspire Dashboard** will open automatically, providing links to the Web frontend and real-time logs for all services.

## 🛠️ Technology Stack
- **Backend:** C# / .NET 10 / ASP.NET Core
- **Frontend:** Blazor
- **Orchestration:** .NET Aspire
- **Data:** Entity Framework Core / PostgreSQL
- **Messaging:** RabbitMQ
- **Caching:** Redis
- **Patterns:** Domain-Driven Design (DDD), Event-Driven Architecture, Clean Architecture

---
*Created with the help of Gemini CLI.*
