namespace MiniMes.Production.Application.ProductionOrders;

public sealed record AddProductionOperationCommand(
    Guid ProductionOrderId,
    int Sequence,
    string Code,
    string Description,
    Guid WorkCenterId,
    decimal PlannedQuantity,
    int? TargetCycleTimeSeconds
);
