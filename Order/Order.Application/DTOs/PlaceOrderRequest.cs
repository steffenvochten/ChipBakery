namespace Order.Application.DTOs;

/// <summary>
/// Internal input model for placing a new order.
/// Validated by <see cref="Order.Application.Validators.PlaceOrderValidator"/>.
/// This is an internal Application DTO — it does NOT reference ChipBakery.Shared.OrderRequest.
/// Mapping from ChipBakery.Shared.OrderRequest happens at the API boundary (OrderEndpoints).
/// </summary>
public record PlaceOrderRequest(
    Guid ProductId,
    int Quantity,
    string CustomerName);
