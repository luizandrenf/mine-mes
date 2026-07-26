using MiniMes.Production.Application.Abstractions;
using MiniMes.Production.Application.ProductionOrders;
using MiniMes.Production.Application.Products;
using MiniMes.Production.Domain.Entities;
using MiniMes.Production.Domain.Enums;
using MiniMes.Production.Domain.Exceptions;
using Xunit;

namespace MiniMes.UnitTests;

public class ProductionOrderServiceTests
{
    private sealed class FakeProductionOrderRepository : IProductionOrderRepository
    {
        public List<ProductionOrder> Added { get; } = new();

        public void Add(ProductionOrder order) => Added.Add(order);

        public Task<IReadOnlyList<ProductionOrder>> GetAllAsync(
            CancellationToken cancellationToken
        ) => Task.FromResult<IReadOnlyList<ProductionOrder>>(Added);

        public Task<ProductionOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Added.FirstOrDefault(o => o.Id == id));

        public Task<ProductionOrder?> GetByIdForUpdateAsync(
            Guid id,
            CancellationToken cancellationToken
        ) => Task.FromResult(Added.FirstOrDefault(o => o.Id == id));

        public Task<bool> OrderNumberExistsAsync(
            string orderNumber,
            CancellationToken cancellationToken
        ) => Task.FromResult(Added.Any(o => o.OrderNumber == orderNumber));
    }

    private sealed class FakeProductRepository(Product? product) : IProductRepository
    {
        public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(product);

        public Task<Product?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(product);

        public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Product>>(product is null ? [] : [product]);

        public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken) =>
            Task.FromResult(product is not null);

        public void Add(Product product) { }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCallCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveCallCount++;
            return Task.FromResult(1);
        }
    }

    private static CreateProductionOrderCommand Command() =>
        new(
            OrderNumber: " op-0001 ",
            ProductId: Guid.NewGuid(),
            PlannedQuantity: 100,
            Priority: 1,
            PlannedStartAt: null,
            PlannedEndAt: null
        );

    private static ProductionOrderService Service(
        FakeProductionOrderRepository repository,
        FakeUnitOfWork unitOfWork
    ) =>
        new(
            repository,
            new FakeProductRepository(new Product("P-1", "Active product")),
            unitOfWork
        );

    private static (
        ProductionOrderService Service,
        ProductionOrder Order,
        FakeUnitOfWork UnitOfWork
    ) SeededOrder()
    {
        var repository = new FakeProductionOrderRepository();
        var order = new ProductionOrder(
            "OP-0001",
            Guid.NewGuid(),
            plannedQuantity: 100,
            priority: 1
        );
        repository.Add(order);

        var unitOfWork = new FakeUnitOfWork();

        return (Service(repository, unitOfWork), order, unitOfWork);
    }

    [Fact]
    public async Task CreateAsync_adds_order_saves_once_and_returns_draft_dto()
    {
        var repository = new FakeProductionOrderRepository();
        var products = new FakeProductRepository(new Product("P-1", "Active product"));
        var unitOfWork = new FakeUnitOfWork();
        var service = new ProductionOrderService(repository, products, unitOfWork);

        ProductionOrderDto dto = await service.CreateAsync(Command(), CancellationToken.None);

        Assert.Equal("Draft", dto.Status);
        Assert.Equal("OP-0001", dto.OrderNumber);
        Assert.Single(repository.Added);
        Assert.Equal(1, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task CreateAsync_throws_and_does_not_save_when_product_missing()
    {
        var repository = new FakeProductionOrderRepository();
        var products = new FakeProductRepository(product: null);
        var unitOfWork = new FakeUnitOfWork();
        var service = new ProductionOrderService(repository, products, unitOfWork);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CreateAsync(Command(), CancellationToken.None)
        );

        Assert.Empty(repository.Added);
        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task CreateAsync_throws_and_does_not_save_when_product_inactive()
    {
        var inactive = new Product("P-2", "Inactive product");
        inactive.Deactivate();

        var repository = new FakeProductionOrderRepository();
        var products = new FakeProductRepository(inactive);
        var unitOfWork = new FakeUnitOfWork();
        var service = new ProductionOrderService(repository, products, unitOfWork);

        await Assert.ThrowsAsync<DomainException>(() =>
            service.CreateAsync(Command(), CancellationToken.None)
        );

        Assert.Empty(repository.Added);
        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task CreateAsync_throws_and_does_not_save_when_order_number_already_exists()
    {
        var repository = new FakeProductionOrderRepository();
        var unitOfWork = new FakeUnitOfWork();
        ProductionOrderService service = Service(repository, unitOfWork);

        await service.CreateAsync(Command(), CancellationToken.None);

        await Assert.ThrowsAsync<DomainException>(() =>
            service.CreateAsync(Command(), CancellationToken.None)
        );

        Assert.Single(repository.Added);
        Assert.Equal(1, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task ReleaseAsync_persists_and_saves_once()
    {
        (ProductionOrderService service, ProductionOrder order, FakeUnitOfWork unitOfWork) =
            SeededOrder();

        await service.ReleaseAsync(order.Id, CancellationToken.None);

        Assert.Equal(ProductionOrderStatus.Released, order.Status);
        Assert.NotNull(order.ReleasedAt);
        Assert.Equal(1, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task ReleaseAsync_throws_and_does_not_save_when_order_missing()
    {
        (ProductionOrderService service, _, FakeUnitOfWork unitOfWork) = SeededOrder();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.ReleaseAsync(Guid.NewGuid(), CancellationToken.None)
        );

        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task StartAsync_throws_and_does_not_save_when_order_is_draft()
    {
        (ProductionOrderService service, ProductionOrder order, FakeUnitOfWork unitOfWork) =
            SeededOrder();

        await Assert.ThrowsAsync<DomainException>(() =>
            service.StartAsync(order.Id, CancellationToken.None)
        );

        Assert.Equal(ProductionOrderStatus.Draft, order.Status);
        Assert.Equal(0, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task CompleteAsync_persists_after_release_and_start()
    {
        (ProductionOrderService service, ProductionOrder order, FakeUnitOfWork unitOfWork) =
            SeededOrder();

        await service.ReleaseAsync(order.Id, CancellationToken.None);
        await service.StartAsync(order.Id, CancellationToken.None);
        await service.CompleteAsync(order.Id, CancellationToken.None);

        Assert.Equal(ProductionOrderStatus.Completed, order.Status);
        Assert.Equal(3, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task CancelAsync_throws_and_does_not_save_when_order_completed()
    {
        (ProductionOrderService service, ProductionOrder order, FakeUnitOfWork unitOfWork) =
            SeededOrder();

        await service.ReleaseAsync(order.Id, CancellationToken.None);
        await service.StartAsync(order.Id, CancellationToken.None);
        await service.CompleteAsync(order.Id, CancellationToken.None);

        await Assert.ThrowsAsync<DomainException>(() =>
            service.CancelAsync(order.Id, CancellationToken.None)
        );

        Assert.Equal(ProductionOrderStatus.Completed, order.Status);
        Assert.Equal(3, unitOfWork.SaveCallCount);
    }

    [Fact]
    public async Task CancelAsync_persists_when_order_is_draft()
    {
        (ProductionOrderService service, ProductionOrder order, FakeUnitOfWork unitOfWork) =
            SeededOrder();

        await service.CancelAsync(order.Id, CancellationToken.None);

        Assert.Equal(ProductionOrderStatus.Cancelled, order.Status);
        Assert.Equal(1, unitOfWork.SaveCallCount);
    }
}
