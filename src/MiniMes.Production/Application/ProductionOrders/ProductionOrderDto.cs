using MiniMes.Production.Domain.Entities;

namespace MiniMes.Production.Application.ProductionOrders;

/// <param name="Status">One of: Draft, Released, InProgress, Completed, Cancelled.</param>
public sealed record ProductionOrderDto(
    Guid Id,
    string OrderNumber,
    Guid ProductId,
    decimal PlannedQuantity,
    string Status,
    int Priority,
    DateTime? PlannedStartAt,
    DateTime? PlannedEndAt,
    DateTime CreatedAt,
    DateTime? ReleasedAt,
    IReadOnlyList<ProductionOperationDto> Operations
)
{
    public static ProductionOrderDto From(ProductionOrder order) =>
        new(
            order.Id,
            order.OrderNumber,
            order.ProductId,
            order.PlannedQuantity,
            order.Status.ToString(),
            order.Priority,
            order.PlannedStartAt,
            order.PlannedEndAt,
            order.CreatedAt,
            order.ReleasedAt,
            order
                .Operations.OrderBy(operation => operation.Sequence)
                .Select(ProductionOperationDto.From)
                .ToList()
        );
}
