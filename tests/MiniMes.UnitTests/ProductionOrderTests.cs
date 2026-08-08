using MiniMes.Production.Domain.Entities;
using MiniMes.Production.Domain.Enums;
using MiniMes.Production.Domain.Exceptions;
using Xunit;

namespace MiniMes.UnitTests;

public class ProductionOrderTests
{
    private static ProductionOrder NewDraftOrder() =>
        new(orderNumber: "OP-0001", productId: Guid.NewGuid(), plannedQuantity: 100, priority: 1);

    private static ProductionOrder NewReleasableOrder()
    {
        var order = NewDraftOrder();
        order.AddOperation(10, "OP-10", "Cut", Guid.NewGuid(), 100);
        return order;
    }

    private static ProductionOrder NewCompletableOrder()
    {
        var order = NewReleasableOrder();
        order.Release();
        order.Start();
        Guid operationId = order.Operations.Single().Id;
        order.StartOperation(operationId);
        order.CompleteOperation(operationId);
        return order;
    }

    [Fact]
    public void New_order_starts_in_draft()
    {
        var order = NewDraftOrder();

        Assert.Equal(ProductionOrderStatus.Draft, order.Status);
        Assert.Null(order.ReleasedAt);
        Assert.NotEqual(Guid.Empty, order.Id);
        Assert.NotEqual(Guid.Empty, order.Version);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Constructor_rejects_non_positive_quantity(decimal quantity)
    {
        Assert.Throws<DomainException>(() =>
            new ProductionOrder("OP-0001", Guid.NewGuid(), quantity, priority: 1)
        );
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_rejects_blank_order_number(string orderNumber)
    {
        Assert.Throws<DomainException>(() =>
            new ProductionOrder(orderNumber, Guid.NewGuid(), plannedQuantity: 100, priority: 1)
        );
    }

    [Fact]
    public void Release_from_draft_moves_to_released_and_rotates_version()
    {
        var order = NewReleasableOrder();
        var versionBefore = order.Version;

        order.Release();

        Assert.Equal(ProductionOrderStatus.Released, order.Status);
        Assert.NotNull(order.ReleasedAt);
        Assert.NotEqual(versionBefore, order.Version);
    }

    [Fact]
    public void Release_outside_draft_throws()
    {
        var order = NewReleasableOrder();
        order.Release();

        Assert.Throws<DomainException>(() => order.Release());
    }

    [Fact]
    public void Start_from_released_moves_to_in_progress()
    {
        var order = NewReleasableOrder();
        order.Release();

        order.Start();

        Assert.Equal(ProductionOrderStatus.InProgress, order.Status);
    }

    [Fact]
    public void Start_from_draft_throws()
    {
        var order = NewDraftOrder();

        Assert.Throws<DomainException>(() => order.Start());
    }

    [Fact]
    public void Complete_from_in_progress_moves_to_completed()
    {
        var order = NewCompletableOrder();

        order.Complete();

        Assert.Equal(ProductionOrderStatus.Completed, order.Status);
    }

    [Fact]
    public void Complete_from_released_throws()
    {
        var order = NewReleasableOrder();
        order.Release();

        Assert.Throws<DomainException>(() => order.Complete());
    }

    [Fact]
    public void Cancel_from_draft_moves_to_cancelled()
    {
        var order = NewDraftOrder();

        order.Cancel();

        Assert.Equal(ProductionOrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void Cancel_from_released_moves_to_cancelled()
    {
        var order = NewReleasableOrder();
        order.Release();

        order.Cancel();

        Assert.Equal(ProductionOrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void Cancel_from_in_progress_throws()
    {
        var order = NewReleasableOrder();
        order.Release();
        order.Start();

        Assert.Throws<DomainException>(() => order.Cancel());
    }

    [Fact]
    public void Cancel_from_completed_throws()
    {
        var order = NewCompletableOrder();
        order.Complete();

        Assert.Throws<DomainException>(() => order.Cancel());
    }

    [Fact]
    public void AddOperation_returns_pending_operation_and_rotates_version()
    {
        var order = NewDraftOrder();
        var versionBefore = order.Version;

        ProductionOperation operation = order.AddOperation(10, "OP-10", "Cut", Guid.NewGuid(), 100);

        Assert.Equal(ProductionOperationStatus.Pending, operation.Status);
        Assert.Equal(order.Id, operation.ProductionOrderId);
        Assert.Single(order.Operations);
        Assert.NotEqual(versionBefore, order.Version);
    }

    [Fact]
    public void AddOperation_rejects_duplicated_sequence()
    {
        var order = NewReleasableOrder();

        Assert.Throws<DomainException>(() =>
            order.AddOperation(10, "OP-11", "Drill", Guid.NewGuid(), 50)
        );
    }

    [Fact]
    public void AddOperation_outside_draft_throws()
    {
        var order = NewReleasableOrder();
        order.Release();

        Assert.Throws<DomainException>(() =>
            order.AddOperation(20, "OP-20", "Drill", Guid.NewGuid(), 50)
        );
    }

    [Fact]
    public void Release_without_operations_throws()
    {
        var order = NewDraftOrder();

        Assert.Throws<DomainException>(() => order.Release());
        Assert.Equal(ProductionOrderStatus.Draft, order.Status);
    }

    [Fact]
    public void Release_with_one_operation_succeeds()
    {
        var order = NewReleasableOrder();

        order.Release();

        Assert.Equal(ProductionOrderStatus.Released, order.Status);
    }

    [Fact]
    public void StartOperation_requires_order_in_progress()
    {
        var order = NewReleasableOrder();
        order.Release();

        Assert.Throws<DomainException>(() => order.StartOperation(order.Operations.Single().Id));
    }

    [Fact]
    public void StartOperation_throws_when_previous_operation_not_completed()
    {
        var order = NewReleasableOrder();
        ProductionOperation second = order.AddOperation(20, "OP-20", "Drill", Guid.NewGuid(), 100);
        order.Release();
        order.Start();

        Assert.Throws<DomainException>(() => order.StartOperation(second.Id));
    }

    [Fact]
    public void StartOperation_succeeds_when_previous_operation_completed()
    {
        var order = NewReleasableOrder();
        ProductionOperation first = order.Operations.Single();
        ProductionOperation second = order.AddOperation(20, "OP-20", "Drill", Guid.NewGuid(), 100);
        order.Release();
        order.Start();
        order.StartOperation(first.Id);
        order.CompleteOperation(first.Id);

        order.StartOperation(second.Id);

        Assert.Equal(ProductionOperationStatus.InProgress, second.Status);
    }

    [Fact]
    public void StartOperation_succeeds_when_previous_operation_cancelled()
    {
        var order = NewReleasableOrder();
        ProductionOperation first = order.Operations.Single();
        ProductionOperation second = order.AddOperation(20, "OP-20", "Drill", Guid.NewGuid(), 100);
        order.Release();
        order.Start();
        order.CancelOperation(first.Id);

        order.StartOperation(second.Id);

        Assert.Equal(ProductionOperationStatus.InProgress, second.Status);
    }

    [Fact]
    public void StartOperation_unknown_id_throws_not_found()
    {
        var order = NewReleasableOrder();
        order.Release();
        order.Start();

        Assert.Throws<NotFoundException>(() => order.StartOperation(Guid.NewGuid()));
    }

    [Fact]
    public void Complete_throws_when_any_operation_is_pending()
    {
        var order = NewReleasableOrder();
        order.AddOperation(20, "OP-20", "Drill", Guid.NewGuid(), 100);
        order.Release();
        order.Start();

        Assert.Throws<DomainException>(() => order.Complete());
        Assert.Equal(ProductionOrderStatus.InProgress, order.Status);
    }

    [Fact]
    public void Complete_succeeds_when_all_operations_completed()
    {
        var order = NewReleasableOrder();
        ProductionOperation first = order.Operations.Single();
        ProductionOperation second = order.AddOperation(20, "OP-20", "Drill", Guid.NewGuid(), 100);
        order.Release();
        order.Start();
        order.StartOperation(first.Id);
        order.CompleteOperation(first.Id);
        order.StartOperation(second.Id);
        order.CompleteOperation(second.Id);

        order.Complete();

        Assert.Equal(ProductionOrderStatus.Completed, order.Status);
    }

    [Fact]
    public void Complete_succeeds_when_remaining_operations_are_cancelled()
    {
        var order = NewReleasableOrder();
        ProductionOperation first = order.Operations.Single();
        ProductionOperation second = order.AddOperation(20, "OP-20", "Drill", Guid.NewGuid(), 100);
        order.Release();
        order.Start();
        order.StartOperation(first.Id);
        order.CompleteOperation(first.Id);
        order.CancelOperation(second.Id);

        order.Complete();

        Assert.Equal(ProductionOrderStatus.Completed, order.Status);
    }
}
