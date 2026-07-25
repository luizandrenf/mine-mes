using MiniMes.Api.Domain.Entities;

namespace MiniMes.Api.Application.ProductionOrders;

// Porta definida pela camada Application; a Infrastructure implementa (esconde o EF Core).
// Operações úteis do domínio, não um CRUD genérico.
public interface IProductionOrderRepository
{
    Task<IReadOnlyList<ProductionOrder>> GetAllAsync(CancellationToken cancellationToken);

    Task<ProductionOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    void Add(ProductionOrder order);
}
