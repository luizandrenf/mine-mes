using Microsoft.EntityFrameworkCore;
using MiniMes.Api.Models;

namespace MiniMes.Api.Data;

public class MiniMesDbContext : DbContext
{
    public MiniMesDbContext(DbContextOptions<MiniMesDbContext> options) : base(options){}

    public DbSet<ProductionOrder> ProductionOrders => Set<ProductionOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProductionOrder>(entity =>
        {
         entity.ToTable("production_orders");

         entity.HasKey(order => order.Id);

         entity
            .Property(order => order.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

         entity
            .Property(order => order.ProductCode)
            .HasColumnName("product_code")
            .HasMaxLength(50)
            .IsRequired();

        entity
            .Property(order => order.PlannedQuantity)
            .HasColumnName("planned_quantity")
            .IsRequired();

        entity
            .Property(order => order.Status)
            .HasColumnName("status")
            .HasMaxLength(30)
            .IsRequired();

        entity
            .Property(order => order.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        entity.HasIndex(order => order.ProductCode);

        });
    }
}