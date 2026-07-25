namespace MiniMes.Api.Application.ProductionOrders;

// Command = intenção do caso de uso, na camada Application. É um record (imutável, igualdade por valor).
// Diferente do Request HTTP: aqui não há atributos de validação nem acoplamento ao ASP.NET.
public sealed record CreateProductionOrderCommand(
    string OrderNumber,
    Guid ProductId,
    decimal PlannedQuantity,
    int Priority,
    DateTime? PlannedStartAt,
    DateTime? PlannedEndAt
);
