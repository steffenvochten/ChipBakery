var builder = DistributedApplication.CreateBuilder(args);

// ==========================================
// 1. Infrastructure (Containers & Backing Services)
// ==========================================

// PostgreSQL Server with separate databases for each service
var postgres = builder.AddPostgres("postgres")
                      .WithPgAdmin(); // Optional: Includes pgAdmin for easy management

var orderDb = postgres.AddDatabase("orderdb");
var warehouseDb = postgres.AddDatabase("warehousedb");
var supplierDb = postgres.AddDatabase("supplierdb");
var inventoryDb = postgres.AddDatabase("inventorydb");
var loyaltyDb = postgres.AddDatabase("loyaltydb");

// Live tracking cache and event messaging
var redis = builder.AddRedis("redis")
                   .WithRedisCommander(); // Optional: UI for viewing Redis data

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
                      .WithManagementPlugin(); // Optional: UI for monitoring RabbitMQ queues

// ==========================================
// 2. Core Services (Web APIs)
// ==========================================

var inventoryService = builder.AddProject<Projects.Inventory_Service>("inventory-service")
    .WithReference(inventoryDb)
    .WaitFor(inventoryDb);

var warehouseService = builder.AddProject<Projects.Warehouse_Service>("warehouse-service")
    .WithReference(warehouseDb)
    .WaitFor(warehouseDb);

var supplierService = builder.AddProject<Projects.Supplier_Service>("supplier-service")
    .WithReference(supplierDb)
    .WaitFor(supplierDb);

var loyaltyService = builder.AddProject<Projects.Loyalty_Service>("loyalty-service")
    .WithReference(loyaltyDb)
    .WaitFor(loyaltyDb);

var orderService = builder.AddProject<Projects.Order_Service>("order-service")
    .WithReference(orderDb)
    .WaitFor(orderDb)
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq); // Needs RabbitMQ to publish "OrderPlaced" events

// ==========================================
// 3. Background Worker
// ==========================================

// Consumes "OrderPlaced" events from RabbitMQ and stores tracking schedules in Redis
var productionWorker = builder.AddProject<Projects.Production_Service>("production-service")
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithReference(redis)
    .WaitFor(redis);

// ==========================================
// 4. Frontend (Blazor Web)
// ==========================================

var webFrontend = builder.AddProject<Projects.ChipBakery_Web>("web")
    .WithReference(orderService)
    .WaitFor(orderService)
    .WithReference(inventoryService)
    .WaitFor(inventoryService)
    .WithExternalHttpEndpoints(); // Exposes the frontend to your local browser

builder.Build().Run();
