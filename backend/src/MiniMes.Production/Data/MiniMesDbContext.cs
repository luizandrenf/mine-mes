using Microsoft.EntityFrameworkCore;
using MiniMes.Production.Domain.Entities;

namespace MiniMes.Production.Data;

public sealed class MiniMesDbContext(DbContextOptions<MiniMesDbContext> options)
    : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    public DbSet<ProductionOrder> ProductionOrders => Set<ProductionOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MiniMesDbContext).Assembly);
    }
}
