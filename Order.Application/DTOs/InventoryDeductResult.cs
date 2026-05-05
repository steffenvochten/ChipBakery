namespace Order.Application.DTOs;

/// <summary>
/// Result returned by <see cref="Order.Application.Interfaces.IInventoryClient.DeductStockAsync"/>.
/// </summary>
/// <param name="Success">Whether the deduction succeeded.</param>
/// <param name="UnitPrice">The unit price of the product at the time of deduction (used to calculate TotalPrice).</param>
/// <param name="ErrorMessage">Populated when Success is false.</param>
public record InventoryDeductResult(bool Success, decimal UnitPrice, string? ErrorMessage = null);
