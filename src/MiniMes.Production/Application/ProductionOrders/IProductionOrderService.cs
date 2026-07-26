namespace MiniMes.Production.Application.ProductionOrders;

public interface IProductionOrderService
{
    Task<ProductionOrderDto> CreateAsync(
        CreateProductionOrderCommand command,
        CancellationToken cancellationToken
    );

    Task<IReadOnlyList<ProductionOrderDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<ProductionOrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task ReleaseAsync(Guid id, CancellationToken cancellationToken);

    Task StartAsync(Guid id, CancellationToken cancellationToken);

    Task CompleteAsync(Guid id, CancellationToken cancellationToken);

    Task CancelAsync(Guid id, CancellationToken cancellationToken);
}
