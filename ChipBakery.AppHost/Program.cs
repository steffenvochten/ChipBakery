var builder = DistributedApplication.CreateBuilder(args);

// ==========================================
// 1. Infrastructure (Containers & Backing Services)
// ==========================================

// PostgreSQL Server with separate databases for each service
var postgres = builder.AddPostgres("postgres")
                      .WithContainerName("chipbakery-postgres")
                      .WithLifetime(ContainerLifetime.Persistent)
                      .WithDataVolume()
                      .WithPgAdmin(c => c.WithContainerName("chipbakery-pgadmin"));

var orderDb = postgres.AddDatabase("orderdb");
var warehouseDb = postgres.AddDatabase("warehousedb");
var supplierDb = postgres.AddDatabase("supplierdb");
var inventoryDb = postgres.AddDatabase("inventorydb");
var loyaltyDb = postgres.AddDatabase("loyaltydb");
var productionDb = postgres.AddDatabase("productiondb");

// Ollama — local LLM server for AI agent decisions
var ollama = builder.AddContainer("ollama", "ollama/ollama")
                    .WithContainerName("chipbakery-ollama")
                    .WithLifetime(ContainerLifetime.Persistent)
                    .WithVolume("chipbakery-ollama-models", "/root/.ollama")
                    .WithHttpEndpoint(port: 11434, targetPort: 11434, name: "http");

// Live tracking cache and event messaging
var redis = builder.AddRedis("redis")
                   .WithContainerName("chipbakery-redis")
                   .WithLifetime(ContainerLifetime.Persistent)
                   .WithRedisCommander(c => c.WithContainerName("chipbakery-redis-commander"));

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
                      .WithContainerName("chipbakery-rabbitmq")
                      .WithLifetime(ContainerLifetime.Persistent)
                      .WithManagementPlugin();

// ==========================================
// 2. Core Services (Web APIs)
// ==========================================

var inventoryService = builder.AddProject<Projects.Inventory_Service>("inventory-service", launchProfileName: "https")
    .WithReference(inventoryDb)
    .WaitFor(inventoryDb)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq);

var warehouseService = builder.AddProject<Projects.Warehouse_Service>("warehouse-service", launchProfileName: "https")
    .WithReference(warehouseDb)
    .WaitFor(warehouseDb)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq);

var supplierService = builder.AddProject<Projects.Supplier_Service>("supplier-service", launchProfileName: "https")
    .WithReference(supplierDb)
    .WaitFor(supplierDb)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq);

var loyaltyService = builder.AddProject<Projects.Loyalty_Service>("loyalty-service", launchProfileName: "https")
    .WithReference(loyaltyDb)
    .WaitFor(loyaltyDb)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq);

var orderService = builder.AddProject<Projects.Order_Service>("order-service", launchProfileName: "https")
    .WithReference(orderDb)
    .WaitFor(orderDb)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithReference(inventoryService) // Needs to discover inventory-service to deduct stock
    .WithReference(warehouseService); // Needs to discover warehouse-service to check recipe

// ==========================================
// 3. Background Worker
// ==========================================

// Consumes "OrderPlaced" events from RabbitMQ and stores tracking schedules in Redis.
// Also runs BakingProgressWorker which calls Warehouse.Service to atomically consume
// recipe ingredients when a job starts — hence the warehouse reference below.
var productionWorker = builder.AddProject<Projects.Production_Service>("production-service", launchProfileName: "https")
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(productionDb)
    .WaitFor(productionDb)
    .WithReference(warehouseService)
    .WaitFor(warehouseService);

// ==========================================
// 4. Agents Service
// ==========================================

// Hosts autonomous agent loops (ClientAgent, SupplierAgent, WarehouseManagerAgent, BakerAgent)
// and the SignalR AgentActivityHub that streams every agent action to the Web frontend.
var agentsService = builder.AddProject<Projects.Agents_Service>("agents-service", launchProfileName: "https")
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithReference(warehouseService)
    .WaitFor(warehouseService)
    .WithReference(inventoryService)
    .WaitFor(inventoryService)
    .WithReference(orderService)
    .WaitFor(orderService)
    .WithReference(supplierService)
    .WaitFor(supplierService)
    .WithReference(productionWorker)
    .WaitFor(productionWorker)
    .WithEnvironment("OLLAMA_BASE_URL", ollama.GetEndpoint("http"));

var catalogService = builder.AddProject<Projects.Catalog_Service>("catalog-service", launchProfileName: "https")
    .WithReference(inventoryService)
    .WithReference(warehouseService)
    .WithReference(agentsService);

// ==========================================
// 5. Frontend (Blazor Web)
// ==========================================

var webFrontend = builder.AddProject<Projects.ChipBakery_Web>("web", launchProfileName: "https")
    .WithReference(orderService)
    .WaitFor(orderService)
    .WithReference(inventoryService)
    .WaitFor(inventoryService)
    .WithReference(warehouseService)
    .WaitFor(warehouseService)
    .WithReference(loyaltyService)
    .WaitFor(loyaltyService)
    .WithReference(supplierService)
    .WaitFor(supplierService)
    .WithReference(productionWorker)
    .WaitFor(productionWorker)
    .WithReference(agentsService)
    .WaitFor(agentsService)
    .WithReference(catalogService)
    .WithExternalHttpEndpoints(); // Exposes the frontend to your local browser

builder.Build().Run();
