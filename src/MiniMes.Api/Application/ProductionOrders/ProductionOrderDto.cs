using MiniMes.Api.Domain.Entities;

namespace MiniMes.Api.Application.ProductionOrders;

// DTO de resposta. Não expõe a entidade nem o Version (token interno de concorrência).
// Status vira texto ("Draft"), não o número do enum.
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
    DateTime? ReleasedAt
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
            order.ReleasedAt
        );
}
