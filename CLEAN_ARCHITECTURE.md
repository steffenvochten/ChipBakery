# ChipBakery — Clean Architecture Implementation Pattern

> **Reference implementations**: `Inventory.Service` (fully built) and `Order.Service` (fully built).
> All other core domain services should follow this pattern exactly.
>
> **Exception**: `Agents.Service` intentionally bypasses this 4-project structure as it functions primarily as an autonomous worker/integration layer and LLM client without its own domain persistence.

> **`IEventPublisher` lives in `ChipBakery.Shared`** — NOT in each service's Domain project.
> This was decided to avoid interface duplication across services (KI updated 2026-05-05).
> Domain projects reference `ChipBakery.Shared` instead of being zero-dependency.

---

## 1. Project Structure Per Service

Each service is split into **4 separate projects**. Never collapse these into folders within one project — the separate projects enforce dependency rules at compile time.

```
[Service].Domain/               ← Pure class library, ZERO external deps
[Service].Application/          ← Business logic, validators, DTOs
[Service].Infrastructure/       ← EF Core, repos, event publisher
[Service].Service/              ← API host (kept as .Service to avoid renaming AppHost refs)
```

> **AppHost naming rule**: The API project is always named `[Service].Service` (not `.API`).  
> This preserves the `Projects.[Service]_Service` reference in `ChipBakery.AppHost/Program.cs` without changes.

### Add all 4 projects to `ChipBakery.slnx`
```xml
<Project Path="[Service].Domain/[Service].Domain.csproj" />
<Project Path="[Service].Application/[Service].Application.csproj" />
<Project Path="[Service].Infrastructure/[Service].Infrastructure.csproj" />
<Project Path="[Service].Service/[Service].Service.csproj" />
```

---

## 2. Project Files & Dependencies

### `[Service].Domain.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <!-- No PackageReferences. No ProjectReferences. Zero dependencies. -->
</Project>
```

### `[Service].Application.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.11.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.0" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\[Service].Domain\[Service].Domain.csproj" />
  </ItemGroup>
</Project>
```
> ⚠️ `Microsoft.Extensions.Logging.Abstractions` is **required** in the Application project — it does not come transitively from a plain `Microsoft.NET.Sdk` class library. Without it, `ILogger<T>` will fail to resolve.

### `[Service].Infrastructure.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Aspire.Npgsql.EntityFrameworkCore.PostgreSQL" Version="13.2.4" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\[Service].Application\[Service].Application.csproj" />
    <ProjectReference Include="..\[Service].Domain\[Service].Domain.csproj" />
  </ItemGroup>
</Project>
```

### `[Service].Service.csproj` (API)
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.7" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\ChipBakery.ServiceDefaults\ChipBakery.ServiceDefaults.csproj" />
    <ProjectReference Include="..\ChipBakery.Shared\ChipBakery.Shared.csproj" />
    <ProjectReference Include="..\[Service].Domain\[Service].Domain.csproj" />
    <ProjectReference Include="..\[Service].Application\[Service].Application.csproj" />
    <ProjectReference Include="..\[Service].Infrastructure\[Service].Infrastructure.csproj" />
  </ItemGroup>
</Project>
```
> The API project references all 3 layers because it's the composition root. The Aspire Npgsql package is **NOT** in the API — it lives in Infrastructure. The DependencyInjection class in Infrastructure uses `IHostApplicationBuilder` (not `IServiceCollection`) to call `builder.AddNpgsqlDbContext<TContext>(...)`.

---

## 3. Domain Layer Structure

```
[Service].Domain/
├── Entities/
│   └── [Entity].cs                     ← Anemic POCO. No behavior methods.
├── Interfaces/
│   ├── I[Entity]Repository.cs          ← Repository contract
│   └── IEventPublisher.cs              ← Generic event publisher
├── Events/
│   ├── [Entity]CreatedEvent.cs         ← Immutable records
│   ├── [Entity]DeletedEvent.cs
│   └── StockDeductedEvent.cs           ← (inventory-specific examples)
└── Exceptions/
    ├── DomainException.cs              ← Abstract base
    ├── [Entity]NotFoundException.cs    ← Thrown by service → 404
    └── InsufficientStockException.cs   ← Thrown by service → 409 (if applicable)
```

**Entity design rule**: Entities are anemic POCOs. No domain behavior methods — all logic lives in the Application service layer. This was an explicit user preference.

**Repository interface pattern**:
```csharp
public interface I[Entity]Repository
{
    Task<Entity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Entity>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Entity item, CancellationToken ct = default);
    void Update(Entity item);      // Sync — leverages EF change tracking
    void Delete(Entity item);      // Sync — leverages EF change tracking
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```
> `Update` and `Delete` are synchronous — they operate on already-tracked entities via EF change tracking. No round-trip re-fetch needed. `SaveChangesAsync` is on the repository for simplicity; extract `IUnitOfWork` later if services span multiple repositories.

---

## 4. Application Layer Structure

```
[Service].Application/
├── DependencyInjection.cs              ← AddApplication() extension
├── DTOs/
│   ├── [Entity]Dto.cs                  ← Read model
│   ├── Create[Entity]Request.cs        ← Write model
│   └── Update[Entity]Request.cs        ← Write model
├── Interfaces/
│   └── I[Entity]Service.cs             ← Service contract
├── Mapping/
│   └── [Entity]MappingExtensions.cs    ← Static ToDto() / ToDtoList() extensions
├── Services/
│   └── [Entity]Service.cs              ← Implementation
└── Validators/
    ├── Create[Entity]Validator.cs
    ├── Update[Entity]Validator.cs
    └── Deduct[Entity]Validator.cs      ← (if applicable)
```

**DI registration**:
```csharp
public static IServiceCollection AddApplication(this IServiceCollection services)
{
    services.AddScoped<I[Entity]Service, [Entity]Service>();
    services.AddValidatorsFromAssemblyContaining<[Entity]Service>(); // auto-discovers all validators
    return services;
}
```

**Service implementation pattern**:
- Constructor-inject: `IRepository`, `IEventPublisher`, `IValidator<CreateRequest>`, `IValidator<UpdateRequest>`, `ILogger<Service>`
- Call `await _validator.ValidateAndThrowAsync(request, ct)` at the top of each mutating method
- Throw domain exceptions (`NotFoundException`, `InsufficientStockException`) — the API layer catches and maps them
- Always call `_repository.SaveChangesAsync(ct)` before publishing events
- Use `ILogger` for structured logging of key operations

**Mapping pattern** — no AutoMapper, just static extension methods:
```csharp
public static EntityDto ToDto(this Entity e) => new(e.Id, e.Name, ...);
public static List<EntityDto> ToDtoList(this IEnumerable<Entity> items) => items.Select(i => i.ToDto()).ToList();
```

---

## 5. Infrastructure Layer Structure

```
[Service].Infrastructure/
├── DependencyInjection.cs              ← AddInfrastructure(IHostApplicationBuilder)
├── Events/
│   └── MockEventPublisher.cs           ← Logs events as JSON; replace with RabbitMQ later
└── Persistence/
    ├── [Entity]DbContext.cs
    ├── Configurations/
    │   └── [Entity]Configuration.cs    ← IEntityTypeConfiguration<T>
    └── Repositories/
        └── [Entity]Repository.cs
```

**DI registration — IMPORTANT**: Must extend `IHostApplicationBuilder`, NOT `IServiceCollection`, to get access to Aspire's `AddNpgsqlDbContext`:
```csharp
// ⚠️ Must also add: using Microsoft.Extensions.DependencyInjection;
// It does not come transitively — AddScoped will fail to compile without it.
public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
{
    builder.AddNpgsqlDbContext<[Entity]DbContext>("[servicename]db"); // matches AppHost db name
    builder.Services.AddScoped<I[Entity]Repository, [Entity]Repository>();
    builder.Services.AddScoped<IEventPublisher, MockEventPublisher>();
    return builder;
}
```

**DbContext pattern**:
```csharp
public class [Entity]DbContext(DbContextOptions<[Entity]DbContext> options) : DbContext(options)
{
    public DbSet<Entity> Items => Set<Entity>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new [Entity]Configuration());
    }
}
```

**Repository pattern** — use `AsNoTracking()` for reads, tracking for writes:
```csharp
public async Task<List<Entity>> GetAllAsync(CancellationToken ct = default) =>
    await _context.Items.AsNoTracking().ToListAsync(ct);

public async Task<Entity?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
    await _context.Items.FindAsync([id], ct); // FindAsync checks tracker first — no AsNoTracking here
```

**MockEventPublisher** — always add this with the TODO comment block:
```csharp
// TODO: Replace with RabbitMQ/MassTransit publisher.
// 1. Add NuGet: Aspire.RabbitMQ.Client (or MassTransit.RabbitMQ)
// 2. AppHost: inventoryService.WithReference(rabbitmq).WaitFor(rabbitmq)
// 3. Create RabbitMqEventPublisher : IEventPublisher
// 4. Swap in DependencyInjection.cs
public Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : class
{
    _logger.LogInformation("[MOCK EVENT BUS] Published {EventName}: {Payload}",
        typeof(TEvent).Name, JsonSerializer.Serialize(@event));
    return Task.CompletedTask;
}
```

---

## 6. API Layer Structure

```
[Service].Service/
├── Program.cs                          ← Thin composition root only
├── Endpoints/
│   └── [Entity]Endpoints.cs           ← All routes in one static class
├── Extensions/
│   └── DatabaseInitializer.cs         ← EnsureCreated() / MigrateAsync()
└── Middleware/
    └── GlobalExceptionHandler.cs      ← IExceptionHandler → ProblemDetails
```

**Program.cs template**:
```csharp
builder.AddServiceDefaults();
builder.AddInfrastructure();        // from [Service].Infrastructure
builder.Services.AddApplication(); // from [Service].Application
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();
app.MapDefaultEndpoints();
app.UseExceptionHandler();          // Must come before MapInventoryEndpoints
app.InitializeDatabase();
app.Map[Entity]Endpoints();
app.Run();
```

**Endpoints pattern** — use `MapGroup` with route prefix:
```csharp
var group = app.MapGroup("/api/[service]").WithTags("[Service]");
group.MapGet("/", async (I[Entity]Service svc, CancellationToken ct) => Results.Ok(await svc.GetAllAsync(ct)));
group.MapGet("/{id:guid}", ...).WithName("Get[Entity]ById"); // named for CreatedAtRoute
group.MapPost("/", async (CreateRequest req, I[Entity]Service svc, CancellationToken ct) => {
    var item = await svc.CreateAsync(req, ct);
    return Results.CreatedAtRoute("Get[Entity]ById", new { id = item.Id }, item);
});
```

**ChipBakery.Shared boundary rule**: The endpoints file is the **only** place that references `ChipBakery.Shared` types (`ProductItem`, `OrderRequest`). Map them to/from internal Application DTOs here — the Application layer must never know about `ChipBakery.Shared`.

**GlobalExceptionHandler** — maps domain exceptions to ProblemDetails:
```csharp
// Implements IExceptionHandler (ASP.NET Core 8+ / .NET 10 standard)
var (statusCode, title, detail, type) = exception switch
{
    [Entity]NotFoundException ex    => (404, "Not Found",          ex.Message, "errors/not-found"),
    InsufficientStockException ex   => (409, "Insufficient Stock", ex.Message, "errors/insufficient-stock"),
    ValidationException ex          => (400, "Validation Failed",  string.Join("; ", ex.Errors...), "errors/validation"),
    _                               => (500, "Internal Error",     "Unexpected error.", "errors/internal")
};
```

---

## 7. .http Test File

Always create/update `[Service].Service/[Service].Service.http` with:
- A `@host` variable pointing to the dev port from `launchSettings.json`
- Variables for stable seed GUIDs
- One request block per endpoint
- Both **happy path** and **error case** requests for every endpoint
- `# Expected:` comments on each block

Dev ports are in `[Service].Service/Properties/launchSettings.json`.

---

## 8. Common Gotchas

| Issue | Cause | Fix |
|---|---|---|
| `ILogger<T>` fails to compile in Application project | Plain SDK doesn't include logging | Add `Microsoft.Extensions.Logging.Abstractions` NuGet |
| `AddScoped` fails to compile in Infrastructure DI | Missing using | Add `using Microsoft.Extensions.DependencyInjection;` |
| `FindAsync` compile error | `[id]` collection literal syntax | Use `await _context.Items.FindAsync([id], ct)` |
| AppHost `Projects.X_Service` not found | Renamed API project | Keep API project named `[Service].Service`, not `[Service].API` |
| DB seed data re-inserts on restart | `EnsureCreated()` re-runs | Use stable, hardcoded GUIDs in `HasData()` seed |

---

## 9. Implementation Order (Parallel-Safe)

Build in waves to maximise parallel file creation:

1. **Wave 1** — All Domain files (no deps, fully parallelisable)
2. **Wave 2** — All Application files (DTOs, validators, interface, mapping) in parallel; then service + DI after
3. **Wave 3** — All Infrastructure files (DbContext, config, repo, mock publisher, DI) in parallel
4. **Wave 4** — All API files (endpoints, middleware, db initializer) in parallel; then rewrite Program.cs + .csproj + .slnx
5. **Build** — `dotnet build [Solution].slnx` to verify; fix compile errors before moving on
6. **Delete** — Remove the original flat files (e.g., old `InventoryDbContext.cs` from the Service root)
