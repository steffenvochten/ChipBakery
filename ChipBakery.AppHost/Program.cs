var builder = DistributedApplication.CreateBuilder(args);

// ==========================================
// 1. Infrastructure (Containers & Backing Services)
// ==========================================

// PostgreSQL Server with separate databases for each service
var postgres = builder.AddPostgres("postgres")
                      .WithDataVolume()
                      .WithPgAdmin(); // Optional: Includes pgAdmin for easy management

var orderDb = postgres.AddDatabase("orderdb");
var warehouseDb = postgres.AddDatabase("warehousedb");
var supplierDb = postgres.AddDatabase("supplierdb");
var inventoryDb = postgres.AddDatabase("inventorydb");
var loyaltyDb = postgres.AddDatabase("loyaltydb");
var productionDb = postgres.AddDatabase("productiondb");

// Live tracking cache and event messaging
var redis = builder.AddRedis("redis")
                   .WithRedisCommander(); // Optional: UI for viewing Redis data

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
                      .WithManagementPlugin(); // Optional: UI for monitoring RabbitMQ queues

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

// Consumes "OrderPlaced" events from RabbitMQ and stores tracking schedules in Redis
var productionWorker = builder.AddProject<Projects.Production_Service>("production-service", launchProfileName: "https")
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithReference(redis)
    .WaitFor(redis)
    .WithReference(productionDb)
    .WaitFor(productionDb);

// ==========================================
// 4. Frontend (Blazor Web)
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
    .WithReference(productionWorker)
    .WaitFor(productionWorker)
    .WithExternalHttpEndpoints(); // Exposes the frontend to your local browser

builder.Build().Run();
