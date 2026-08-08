using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MiniMes.Production.Domain.Entities;

namespace MiniMes.Production.Data.Configurations;

public sealed class ProductionOperationConfiguration : IEntityTypeConfiguration<ProductionOperation>
{
    public void Configure(EntityTypeBuilder<ProductionOperation> builder)
    {
        builder.ToTable("production_operations");

        builder.HasKey(operation => operation.Id);

        builder.Property(operation => operation.Id).HasColumnName("id").ValueGeneratedNever();

        builder
            .Property(operation => operation.ProductionOrderId)
            .HasColumnName("production_order_id")
            .IsRequired();

        builder.Property(operation => operation.Sequence).HasColumnName("sequence").IsRequired();

        builder
            .Property(operation => operation.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder
            .Property(operation => operation.Description)
            .HasColumnName("description")
            .HasMaxLength(200)
            .IsRequired();

        // Loose reference to the WorkCenter (Catalog service): no FK, validated by API/event once
        // Catalog exists.
        builder
            .Property(operation => operation.WorkCenterId)
            .HasColumnName("work_center_id")
            .IsRequired();

        builder
            .Property(operation => operation.PlannedQuantity)
            .HasColumnName("planned_quantity")
            .HasPrecision(18, 3)
            .IsRequired();

        builder
            .Property(operation => operation.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder
            .Property(operation => operation.TargetCycleTimeSeconds)
            .HasColumnName("target_cycle_time_seconds");

        builder
            .HasIndex(operation => new { operation.ProductionOrderId, operation.Sequence })
            .IsUnique();
    }
}
