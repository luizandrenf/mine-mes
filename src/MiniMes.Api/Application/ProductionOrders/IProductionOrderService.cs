namespace MiniMes.Api.Application.ProductionOrders;

public interface IProductionOrderService
{
    Task<ProductionOrderDto> CreateAsync(
        CreateProductionOrderCommand command,
        CancellationToken cancellationToken
    );

    Task<IReadOnlyList<ProductionOrderDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<ProductionOrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
