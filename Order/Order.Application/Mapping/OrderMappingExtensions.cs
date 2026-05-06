using ChipBakery.Shared;
using Order.Application.DTOs;
using Order.Domain.Entities;

namespace Order.Application.Mapping;

/// <summary>
/// Extension methods to map between domain entities and application DTOs.
/// No third-party mapping library needed — keeps the dependency graph clean.
/// </summary>
public static class OrderMappingExtensions
{
    public static OrderDto ToDto(this BakeryOrder order) =>
        new(
            order.Id,
            order.CustomerName,
            order.ProductId,
            order.Quantity,
            order.TotalPrice,
            order.Status,
            order.OrderDate);

    public static List<OrderDto> ToDtoList(this IEnumerable<BakeryOrder> orders) =>
        orders.Select(o => o.ToDto()).ToList();
}
