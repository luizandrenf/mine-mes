using MiniMes.Production.Domain.Entities;
using MiniMes.Production.Domain.Enums;
using MiniMes.Production.Domain.Exceptions;
using Xunit;

namespace MiniMes.UnitTests;

public class ProductionOperationTests
{
    private static ProductionOperation NewOperation() =>
        new(
            Guid.NewGuid(),
            sequence: 10,
            code: "OP-10",
            description: "Cut",
            workCenterId: Guid.NewGuid(),
            plannedQuantity: 100
        );

    [Fact]
    public void New_operation_starts_pending()
    {
        var operation = NewOperation();

        Assert.Equal(ProductionOperationStatus.Pending, operation.Status);
        Assert.NotEqual(Guid.Empty, operation.Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_rejects_non_positive_sequence(int sequence)
    {
        Assert.Throws<DomainException>(() =>
            new ProductionOperation(Guid.NewGuid(), sequence, "OP-10", "Cut", Guid.NewGuid(), 100)
        );
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_rejects_blank_code(string code)
    {
        Assert.Throws<DomainException>(() =>
            new ProductionOperation(Guid.NewGuid(), 10, code, "Cut", Guid.NewGuid(), 100)
        );
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_rejects_blank_description(string description)
    {
        Assert.Throws<DomainException>(() =>
            new ProductionOperation(Guid.NewGuid(), 10, "OP-10", description, Guid.NewGuid(), 100)
        );
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Constructor_rejects_non_positive_quantity(decimal quantity)
    {
        Assert.Throws<DomainException>(() =>
            new ProductionOperation(Guid.NewGuid(), 10, "OP-10", "Cut", Guid.NewGuid(), quantity)
        );
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_rejects_non_positive_cycle_time(int cycleTime)
    {
        Assert.Throws<DomainException>(() =>
            new ProductionOperation(
                Guid.NewGuid(),
                10,
                "OP-10",
                "Cut",
                Guid.NewGuid(),
                100,
                cycleTime
            )
        );
    }

    [Fact]
    public void Start_from_pending_moves_to_in_progress()
    {
        var operation = NewOperation();

        operation.Start();

        Assert.Equal(ProductionOperationStatus.InProgress, operation.Status);
    }

    [Fact]
    public void Start_outside_pending_throws()
    {
        var operation = NewOperation();
        operation.Start();

        Assert.Throws<DomainException>(() => operation.Start());
    }

    [Fact]
    public void Complete_from_in_progress_moves_to_completed()
    {
        var operation = NewOperation();
        operation.Start();

        operation.Complete();

        Assert.Equal(ProductionOperationStatus.Completed, operation.Status);
    }

    [Fact]
    public void Complete_outside_in_progress_throws()
    {
        var operation = NewOperation();

        Assert.Throws<DomainException>(() => operation.Complete());
    }

    [Fact]
    public void Cancel_from_pending_moves_to_cancelled()
    {
        var operation = NewOperation();

        operation.Cancel();

        Assert.Equal(ProductionOperationStatus.Cancelled, operation.Status);
    }

    [Fact]
    public void Cancel_from_in_progress_moves_to_cancelled()
    {
        var operation = NewOperation();
        operation.Start();

        operation.Cancel();

        Assert.Equal(ProductionOperationStatus.Cancelled, operation.Status);
    }

    [Fact]
    public void Cancel_completed_throws()
    {
        var operation = NewOperation();
        operation.Start();
        operation.Complete();

        Assert.Throws<DomainException>(() => operation.Cancel());
    }
}
