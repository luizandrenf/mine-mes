namespace MiniMes.Production.Application.ProductionOrders;

public sealed record CreateProductionOrderCommand(
    string OrderNumber,
    Guid ProductId,
    decimal PlannedQuantity,
    int Priority,
    DateTime? PlannedStartAt,
    DateTime? PlannedEndAt
);
