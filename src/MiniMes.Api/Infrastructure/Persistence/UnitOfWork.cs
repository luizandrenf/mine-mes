using MiniMes.Api.Application.Abstractions;
using MiniMes.Api.Data;

namespace MiniMes.Api.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly MiniMesDbContext _dbContext;

    public UnitOfWork(MiniMesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        _dbContext.SaveChangesAsync(cancellationToken);
}
