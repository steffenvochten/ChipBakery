namespace Inventory.Application.DTOs;

/// <summary>
/// Input model for stock deduction requests (called by Order.Service).
/// Validated by <see cref="Inventory.Application.Validators.DeductStockValidator"/>.
/// </summary>
public record DeductStockRequest(
    Guid ProductId,
    int Quantity);
