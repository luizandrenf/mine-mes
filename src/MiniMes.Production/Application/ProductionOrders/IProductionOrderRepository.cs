using MiniMes.Production.Domain.Entities;

namespace MiniMes.Production.Application.ProductionOrders;

public interface IProductionOrderRepository
{
    Task<IReadOnlyList<ProductionOrder>> GetAllAsync(CancellationToken cancellationToken);

    Task<ProductionOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<ProductionOrder?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> OrderNumberExistsAsync(string orderNumber, CancellationToken cancellationToken);

    void Add(ProductionOrder order);
}
