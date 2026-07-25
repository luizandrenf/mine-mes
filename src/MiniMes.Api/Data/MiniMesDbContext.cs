using Microsoft.EntityFrameworkCore;
using MiniMes.Api.Domain.Entities;

namespace MiniMes.Api.Data;

public sealed class MiniMesDbContext : DbContext
{
    public MiniMesDbContext(DbContextOptions<MiniMesDbContext> options)
        : base(options) { }

    public DbSet<ProductionOrder> ProductionOrders => Set<ProductionOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MiniMesDbContext).Assembly);
    }
}
