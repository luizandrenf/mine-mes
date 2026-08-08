using MiniMes.Production.Domain.Entities;

namespace MiniMes.Production.Application.ProductionOrders;

/// <param name="Status">One of: Pending, InProgress, Completed, Cancelled.</param>
public sealed record ProductionOperationDto(
    Guid Id,
    Guid ProductionOrderId,
    int Sequence,
    string Code,
    string Description,
    Guid WorkCenterId,
    decimal PlannedQuantity,
    string Status,
    int? TargetCycleTimeSeconds
)
{
    public static ProductionOperationDto From(ProductionOperation operation) =>
        new(
            operation.Id,
            operation.ProductionOrderId,
            operation.Sequence,
            operation.Code,
            operation.Description,
            operation.WorkCenterId,
            operation.PlannedQuantity,
            operation.Status.ToString(),
            operation.TargetCycleTimeSeconds
        );
}
