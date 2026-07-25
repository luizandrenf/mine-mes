using MiniMes.Production.Domain.Entities;

namespace MiniMes.Production.Application.ProductionOrders;

public interface IProductionOrderRepository
{
    Task<IReadOnlyList<ProductionOrder>> GetAllAsync(CancellationToken cancellationToken);

    Task<ProductionOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    void Add(ProductionOrder order);
}
