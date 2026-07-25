using MiniMes.Production.Application.Abstractions;
using MiniMes.Production.Data;

namespace MiniMes.Production.Infrastructure.Persistence;

public sealed class UnitOfWork(MiniMesDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
