using Microsoft.EntityFrameworkCore;
using MiniMes.Api.Application.ProductionOrders;
using MiniMes.Api.Data;
using MiniMes.Api.Domain.Entities;

namespace MiniMes.Api.Infrastructure.Repositories;

public sealed class ProductionOrderRepository(MiniMesDbContext dbContext) : IProductionOrderRepository
{
    private readonly MiniMesDbContext _dbContext = dbContext;

    public async Task<IReadOnlyList<ProductionOrder>> GetAllAsync(CancellationToken cancellationToken)
    {
        // Leitura: AsNoTracking (não precisamos rastrear para alterar).
        return await _dbContext
            .ProductionOrders.AsNoTracking()
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductionOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext
            .ProductionOrders.AsNoTracking()
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);
    }

    public void Add(ProductionOrder order)
    {
        // Só marca a entidade para inserção; quem persiste é o UnitOfWork (mesmo DbContext).
        _dbContext.ProductionOrders.Add(order);
    }
}
