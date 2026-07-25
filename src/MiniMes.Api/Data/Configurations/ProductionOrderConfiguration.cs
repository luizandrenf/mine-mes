using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniMes.Api.Domain.Entities;

namespace MiniMes.Api.Data.Configurations;

public sealed class ProductionOrderConfiguration : IEntityTypeConfiguration<ProductionOrder>
{
    public void Configure(EntityTypeBuilder<ProductionOrder> builder)
    {
        builder.ToTable("production_orders");

        builder.HasKey(order => order.Id);

        builder.Property(order => order.Id).HasColumnName("id").ValueGeneratedNever();

        builder
            .Property(order => order.OrderNumber)
            .HasColumnName("order_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(order => order.OrderNumber).IsUnique();

        builder.Property(order => order.ProductId).HasColumnName("product_id").IsRequired();

        builder
            .Property(order => order.PlannedQuantity)
            .HasColumnName("planned_quantity")
            .HasPrecision(18, 3)
            .IsRequired();

        // Guarda o enum como texto ("Draft", "Released"...) em vez do número inteiro:
        // legível no banco e resistente a reordenação dos membros do enum.
        builder
            .Property(order => order.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(order => order.Priority).HasColumnName("priority").IsRequired();

        builder
            .Property(order => order.PlannedStartAt)
            .HasColumnName("planned_start_at")
            .HasColumnType("timestamp with time zone");

        builder
            .Property(order => order.PlannedEndAt)
            .HasColumnName("planned_end_at")
            .HasColumnType("timestamp with time zone");

        builder
            .Property(order => order.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder
            .Property(order => order.ReleasedAt)
            .HasColumnName("released_at")
            .HasColumnType("timestamp with time zone");

        builder.Property(order => order.Version).HasColumnName("version").IsConcurrencyToken();
    }
}
