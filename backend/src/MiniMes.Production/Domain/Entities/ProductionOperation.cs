using MiniMes.Production.Domain.Enums;
using MiniMes.Production.Domain.Exceptions;

namespace MiniMes.Production.Domain.Entities;

public class ProductionOperation
{
    // EF Core materialization only.
    private ProductionOperation() { }

    // Public because the test project is another assembly; the only legitimate caller is
    // ProductionOrder.AddOperation.
    public ProductionOperation(
        Guid productionOrderId,
        int sequence,
        string code,
        string description,
        Guid workCenterId,
        decimal plannedQuantity,
        int? targetCycleTimeSeconds = null
    )
    {
        if (sequence <= 0)
        {
            throw new DomainException("Sequence must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException("Operation code is required.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("Operation description is required.");
        }

        if (plannedQuantity <= 0)
        {
            throw new DomainException("Planned quantity must be greater than zero.");
        }

        if (targetCycleTimeSeconds is <= 0)
        {
            throw new DomainException("Target cycle time must be greater than zero.");
        }

        Id = Guid.NewGuid();
        ProductionOrderId = productionOrderId;
        Sequence = sequence;
        Code = code;
        Description = description;
        WorkCenterId = workCenterId;
        PlannedQuantity = plannedQuantity;
        Status = ProductionOperationStatus.Pending;
        TargetCycleTimeSeconds = targetCycleTimeSeconds;
    }

    public Guid Id { get; private set; }

    public Guid ProductionOrderId { get; private set; }

    public int Sequence { get; private set; }

    public string Code { get; private set; } = null!;

    public string Description { get; private set; } = null!;

    public Guid WorkCenterId { get; private set; }

    public decimal PlannedQuantity { get; private set; }

    public ProductionOperationStatus Status { get; private set; }

    public int? TargetCycleTimeSeconds { get; private set; }

    public void Start()
    {
        if (Status != ProductionOperationStatus.Pending)
        {
            throw new DomainException(
                $"Only a pending operation can be started. Current status: {Status}."
            );
        }

        Status = ProductionOperationStatus.InProgress;
    }

    public void Complete()
    {
        if (Status != ProductionOperationStatus.InProgress)
        {
            throw new DomainException(
                $"Only an in-progress operation can be completed. Current status: {Status}."
            );
        }

        Status = ProductionOperationStatus.Completed;
    }

    public void Cancel()
    {
        if (Status == ProductionOperationStatus.Completed)
        {
            throw new DomainException("A completed operation cannot be cancelled.");
        }

        Status = ProductionOperationStatus.Cancelled;
    }
}
