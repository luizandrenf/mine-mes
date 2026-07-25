using Microsoft.EntityFrameworkCore;
using MiniMes.Production.Application.ProductionOrders;
using MiniMes.Production.Data;
using MiniMes.Production.Domain.Entities;

namespace MiniMes.Production.Infrastructure.Repositories;

public sealed class ProductionOrderRepository(MiniMesDbContext dbContext)
    : IProductionOrderRepository
{
    public async Task<IReadOnlyList<ProductionOrder>> GetAllAsync(
        CancellationToken cancellationToken
    ) =>
        await dbContext
            .ProductionOrders.AsNoTracking()
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<ProductionOrder?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken
    ) =>
        await dbContext
            .ProductionOrders.AsNoTracking()
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);

    public void Add(ProductionOrder order) => dbContext.ProductionOrders.Add(order);
}
