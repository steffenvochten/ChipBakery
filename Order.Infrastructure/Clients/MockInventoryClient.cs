using Microsoft.Extensions.Logging;
using Order.Application.DTOs;
using Order.Application.Interfaces;

namespace Order.Infrastructure.Clients;

// ┌─────────────────────────────────────────────────────────────────────────────┐
// │                        MOCK IMPLEMENTATION                                  │
// │                                                                             │
// │  This client simulates a successful Inventory.Service stock deduction.      │
// │  It always returns Success = true with a hardcoded unit price of 3.50.      │
// │                                                                             │
// │  TO REPLACE WITH THE REAL HTTP CLIENT:                                      │
// │  1. Create HttpInventoryClient : IInventoryClient in this folder            │
// │  2. Inject IHttpClientFactory into its constructor                          │
// │  3. Use the "Inventory" named HttpClient (already registered in Program.cs) │
// │  4. Call POST /api/inventory/deduct with ChipBakery.Shared.OrderRequest     │
// │     to deduct stock — check IsSuccessStatusCode for failure                 │
// │  5. Call GET /api/inventory/{productId} to retrieve the real unit price     │
// │     (returns ChipBakery.Shared.ProductItem or internal InventoryItemDto)    │
// │  6. Swap the DI registration in DependencyInjection.cs:                     │
// │         builder.Services.AddScoped<IInventoryClient, HttpInventoryClient>() │
// └─────────────────────────────────────────────────────────────────────────────┘

/// <summary>
/// Development/placeholder implementation of <see cref="IInventoryClient"/>.
/// Always reports a successful deduction with a fixed unit price.
/// Replace with <c>HttpInventoryClient</c> for real Inventory.Service integration.
/// </summary>
public class MockInventoryClient : IInventoryClient
{
    private readonly ILogger<MockInventoryClient> _logger;

    // Hardcoded fallback price used by the mock.
    // In production this comes from Inventory.Service GET /api/inventory/{id}.
    private const decimal MockUnitPrice = 3.50m;

    public MockInventoryClient(ILogger<MockInventoryClient> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<InventoryDeductResult> DeductStockAsync(Guid productId, int quantity, CancellationToken ct = default)
    {
        _logger.LogWarning(
            "[MOCK INVENTORY CLIENT] Simulated deduction of {Quantity} unit(s) for product {ProductId}. " +
            "Using hardcoded unit price of {UnitPrice:C}. " +
            "Replace MockInventoryClient with HttpInventoryClient for real stock validation.",
            quantity, productId, MockUnitPrice);

        return Task.FromResult(new InventoryDeductResult(true, MockUnitPrice));
    }
}
