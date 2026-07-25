using Microsoft.EntityFrameworkCore;
using MiniMes.Production.Application.Products;
using MiniMes.Production.Data;
using MiniMes.Production.Domain.Entities;

namespace MiniMes.Production.Infrastructure.Repositories;

public sealed class ProductRepository(MiniMesDbContext dbContext) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken) =>
        await dbContext
            .Products.AsNoTracking()
            .OrderByDescending(product => product.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext
            .Products.AsNoTracking()
            .FirstOrDefaultAsync(product => product.Id == id, cancellationToken);

    public async Task<Product?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken
    ) =>
        await dbContext.Products.FirstOrDefaultAsync(
            product => product.Id == id,
            cancellationToken
        );

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken) =>
        dbContext.Products.AnyAsync(product => product.Code == code, cancellationToken);

    public void Add(Product product) => dbContext.Products.Add(product);
}
