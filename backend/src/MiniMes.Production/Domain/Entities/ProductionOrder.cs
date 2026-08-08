using MiniMes.Production.Domain.Enums;
using MiniMes.Production.Domain.Exceptions;

namespace MiniMes.Production.Domain.Entities;

public class ProductionOrder
{
    // Backing field name matters: EF Core finds it by convention (see ProductionOrderConfiguration).
    private readonly List<ProductionOperation> _operations = [];

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

    public IReadOnlyCollection<ProductionOperation> Operations => _operations;

    private ProductionOperation Operation(Guid operationId) =>
        _operations.FirstOrDefault(operation => operation.Id == operationId)
        ?? throw new NotFoundException(
            $"Operation {operationId} not found in order {OrderNumber}."
        );

    public ProductionOperation AddOperation(
        int sequence,
        string code,
        string description,
        Guid workCenterId,
        decimal plannedQuantity,
        int? targetCycleTimeSeconds = null
    )
    {
        // ponytail: only Draft accepts operations; open it up to Released when a real use case shows up.
        if (Status != ProductionOrderStatus.Draft)
        {
            throw new DomainException(
                $"Operations can only be added to a draft order. Current status: {Status}."
            );
        }

        if (_operations.Any(operation => operation.Sequence == sequence))
        {
            throw new DomainException(
                $"Sequence {sequence} is already used in order {OrderNumber}."
            );
        }

        var operation = new ProductionOperation(
            productionOrderId: Id,
            sequence: sequence,
            code: code,
            description: description,
            workCenterId: workCenterId,
            plannedQuantity: plannedQuantity,
            targetCycleTimeSeconds: targetCycleTimeSeconds
        );

        _operations.Add(operation);
        Version = Guid.NewGuid();

        return operation;
    }

    public void StartOperation(Guid operationId)
    {
        if (Status != ProductionOrderStatus.InProgress)
        {
            throw new DomainException(
                $"Operations can only be started on an in-progress order. Current status: {Status}."
            );
        }

        ProductionOperation operation = Operation(operationId);

        bool previousPending = _operations.Any(other =>
            other.Sequence < operation.Sequence
            && other.Status != ProductionOperationStatus.Completed
            && other.Status != ProductionOperationStatus.Cancelled
        );

        if (previousPending)
        {
            throw new DomainException(
                $"Operation {operation.Sequence} cannot start while a previous operation is not completed."
            );
        }

        operation.Start();
        Version = Guid.NewGuid();
    }

    public void CompleteOperation(Guid operationId)
    {
        Operation(operationId).Complete();
        Version = Guid.NewGuid();
    }

    public void CancelOperation(Guid operationId)
    {
        Operation(operationId).Cancel();
        Version = Guid.NewGuid();
    }

    public void Release()
    {
        if (Status != ProductionOrderStatus.Draft)
        {
            throw new DomainException(
                $"Only a draft order can be released. Current status: {Status}."
            );
        }

        if (_operations.Count == 0)
        {
            throw new DomainException(
                "A production order requires at least one operation before release."
            );
        }

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

        // ponytail: a cancelled operation does not block completion — reading "all completed"
        // literally would lock the order forever.
        bool pendingOperations = _operations.Any(operation =>
            operation.Status != ProductionOperationStatus.Completed
            && operation.Status != ProductionOperationStatus.Cancelled
        );

        if (pendingOperations)
        {
            throw new DomainException(
                "All operations must be completed or cancelled before completing the order."
            );
        }

        Status = ProductionOrderStatus.Completed;
        Version = Guid.NewGuid();
    }

    // ponytail: no cascade to the operations — add it when something reads their status after
    // the order is cancelled.
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
