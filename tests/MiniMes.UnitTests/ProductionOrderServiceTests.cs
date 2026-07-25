using MiniMes.Production.Application.Abstractions;
using MiniMes.Production.Application.ProductionOrders;
using MiniMes.Production.Application.Products;
using MiniMes.Production.Domain.Entities;
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
}
