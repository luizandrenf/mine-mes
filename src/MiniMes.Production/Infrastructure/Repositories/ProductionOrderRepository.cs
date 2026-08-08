using Microsoft.EntityFrameworkCore;
using MiniMes.Production.Application.ProductionOrders;
using MiniMes.Production.Data;
using MiniMes.Production.Domain.Entities;

namespace MiniMes.Production.Infrastructure.Repositories;

public sealed class ProductionOrderRepository(MiniMesDbContext dbContext)
    : IProductionOrderRepository
{
    // ponytail: Include on GetAll too — few orders, a single join. Split into a summary DTO if the
    // list grows.
    public async Task<IReadOnlyList<ProductionOrder>> GetAllAsync(
        CancellationToken cancellationToken
    ) =>
        await dbContext
            .ProductionOrders.AsNoTracking()
            .Include(order => order.Operations)
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<ProductionOrder?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken
    ) =>
        await dbContext
            .ProductionOrders.AsNoTracking()
            .Include(order => order.Operations)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);

    public void Add(ProductionOrder order) => dbContext.ProductionOrders.Add(order);

    public async Task<ProductionOrder?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken
    ) =>
        await dbContext
            .ProductionOrders.Include(order => order.Operations)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);

    public async Task<bool> OrderNumberExistsAsync(
        string orderNumber,
        CancellationToken cancellationToken
    ) =>
        await dbContext.ProductionOrders.AnyAsync(
            order => order.OrderNumber == orderNumber,
            cancellationToken
        );
}
