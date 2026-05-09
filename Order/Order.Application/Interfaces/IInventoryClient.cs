using Order.Application.DTOs;

namespace Order.Application.Interfaces;

/// <summary>
/// Abstraction for synchronous stock validation and deduction against Inventory.Service.
/// Keeps the Application layer decoupled from HTTP concerns and fully unit-testable.
/// </summary>
public interface IInventoryClient
{
    /// <summary>
    /// Validates that sufficient stock exists, deducts the requested quantity,
    /// and returns the unit price for TotalPrice calculation.
    /// </summary>
    /// <param name="productId">The product to deduct from.</param>
    /// <param name="quantity">The number of units to deduct.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A result containing the unit price on success.
    /// On failure, <see cref="InventoryDeductResult.Success"/> is false and
    /// <see cref="InventoryDeductResult.ErrorMessage"/> contains the reason.
    /// </returns>
    Task<InventoryDeductResult> DeductStockAsync(Guid productId, int quantity, CancellationToken ct = default);
}
