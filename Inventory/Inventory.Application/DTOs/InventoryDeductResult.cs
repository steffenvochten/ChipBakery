namespace Inventory.Application.DTOs;

/// <summary>
/// Result of a stock deduction attempt.
/// Returned to Order.Service so it can avoid an additional GET to fetch the unit price.
/// </summary>
public record InventoryDeductResult(bool Success, decimal UnitPrice, string? Message = null);
