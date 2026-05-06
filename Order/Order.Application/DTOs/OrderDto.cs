using ChipBakery.Shared;

namespace Order.Application.DTOs;

/// <summary>
/// Read model returned to callers for any order query.
/// Contains the full order snapshot including computed TotalPrice.
/// </summary>
public record OrderDto(
    Guid Id,
    string CustomerName,
    Guid ProductId,
    int Quantity,
    decimal TotalPrice,
    OrderStatus Status,
    DateTime OrderDate);
