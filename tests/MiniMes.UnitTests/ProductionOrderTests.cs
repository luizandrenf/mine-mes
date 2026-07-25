using MiniMes.Api.Domain.Entities;
using MiniMes.Api.Domain.Enums;
using MiniMes.Api.Domain.Exceptions;
using Xunit;

namespace MiniMes.UnitTests;

public class ProductionOrderTests
{
    // Helper: cria uma ordem válida em Draft para os cenários.
    private static ProductionOrder NewDraftOrder() =>
        new(orderNumber: "OP-0001", productId: Guid.NewGuid(), plannedQuantity: 100, priority: 1);

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
        var order = NewDraftOrder();
        var versionBefore = order.Version;

        order.Release();

        Assert.Equal(ProductionOrderStatus.Released, order.Status);
        Assert.NotNull(order.ReleasedAt);
        Assert.NotEqual(versionBefore, order.Version);
    }

    [Fact]
    public void Release_outside_draft_throws()
    {
        var order = NewDraftOrder();
        order.Release();

        Assert.Throws<DomainException>(() => order.Release());
    }

    [Fact]
    public void Start_from_released_moves_to_in_progress()
    {
        var order = NewDraftOrder();
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
        var order = NewDraftOrder();
        order.Release();
        order.Start();

        order.Complete();

        Assert.Equal(ProductionOrderStatus.Completed, order.Status);
    }

    [Fact]
    public void Complete_from_released_throws()
    {
        var order = NewDraftOrder();
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
        var order = NewDraftOrder();
        order.Release();

        order.Cancel();

        Assert.Equal(ProductionOrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void Cancel_from_in_progress_throws()
    {
        var order = NewDraftOrder();
        order.Release();
        order.Start();

        Assert.Throws<DomainException>(() => order.Cancel());
    }

    [Fact]
    public void Cancel_from_completed_throws()
    {
        var order = NewDraftOrder();
        order.Release();
        order.Start();
        order.Complete();

        Assert.Throws<DomainException>(() => order.Cancel());
    }
}
