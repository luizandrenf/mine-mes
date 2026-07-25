using MiniMes.Production.Domain.Enums;
using MiniMes.Production.Domain.Exceptions;

namespace MiniMes.Production.Domain.Entities;

public class ProductionOrder
{
    // EF Core materialization only.
    private ProductionOrder() { }

    public ProductionOrder(
        string orderNumber,
        Guid productId,
        decimal plannedQuantity,
        int priority,
        DateTime? plannedStartAt = null,
        DateTime? plannedEndAt = null
    )
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            throw new DomainException("Order number is required.");
        }

        if (plannedQuantity <= 0)
        {
            throw new DomainException("Planned quantity must be greater than zero.");
        }

        Id = Guid.NewGuid();
        OrderNumber = orderNumber;
        ProductId = productId;
        PlannedQuantity = plannedQuantity;
        Priority = priority;
        PlannedStartAt = plannedStartAt;
        PlannedEndAt = plannedEndAt;
        Status = ProductionOrderStatus.Draft;
        CreatedAt = DateTime.UtcNow;
        Version = Guid.NewGuid();
    }

    public Guid Id { get; private set; }

    public string OrderNumber { get; private set; } = null!;

    public Guid ProductId { get; private set; }

    public decimal PlannedQuantity { get; private set; }

    public ProductionOrderStatus Status { get; private set; }

    public int Priority { get; private set; }

    public DateTime? PlannedStartAt { get; private set; }

    public DateTime? PlannedEndAt { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? ReleasedAt { get; private set; }

    public Guid Version { get; private set; }

    public void Release()
    {
        if (Status != ProductionOrderStatus.Draft)
        {
            throw new DomainException(
                $"Only a draft order can be released. Current status: {Status}."
            );
        }

        // ponytail: require at least one operation once ProductionOperation exists.
        Status = ProductionOrderStatus.Released;
        ReleasedAt = DateTime.UtcNow;
        Version = Guid.NewGuid();
    }

    public void Start()
    {
        if (Status != ProductionOrderStatus.Released)
        {
            throw new DomainException(
                $"Only a released order can be started. Current status: {Status}."
            );
        }

        Status = ProductionOrderStatus.InProgress;
        Version = Guid.NewGuid();
    }

    public void Complete()
    {
        if (Status != ProductionOrderStatus.InProgress)
        {
            throw new DomainException(
                $"Only an in-progress order can be completed. Current status: {Status}."
            );
        }

        // ponytail: require all operations completed once they exist.
        Status = ProductionOrderStatus.Completed;
        Version = Guid.NewGuid();
    }

    public void Cancel()
    {
        if (Status != ProductionOrderStatus.Draft && Status != ProductionOrderStatus.Released)
        {
            throw new DomainException(
                $"Only a draft or released order can be cancelled. Current status: {Status}."
            );
        }

        Status = ProductionOrderStatus.Cancelled;
        Version = Guid.NewGuid();
    }
}
