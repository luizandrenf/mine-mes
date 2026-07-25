using MiniMes.Api.Application.Abstractions;
using MiniMes.Api.Application.ProductionOrders;
using MiniMes.Api.Domain.Entities;
using Xunit;

namespace MiniMes.UnitTests;

public class ProductionOrderServiceTests
{
    // Fakes escritos à mão (sem lib de mock, seção 19): implementam as interfaces da Application.
    private sealed class FakeProductionOrderRepository : IProductionOrderRepository
    {
        public List<ProductionOrder> Added { get; } = new();

        public void Add(ProductionOrder order) => Added.Add(order);

        public Task<IReadOnlyList<ProductionOrder>> GetAllAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<ProductionOrder>>(Added);

        public Task<ProductionOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Added.FirstOrDefault(o => o.Id == id));
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

    [Fact]
    public async Task CreateAsync_adds_order_saves_once_and_returns_draft_dto()
    {
        var repository = new FakeProductionOrderRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = new ProductionOrderService(repository, unitOfWork);

        var command = new CreateProductionOrderCommand(
            OrderNumber: " op-0001 ",
            ProductId: Guid.NewGuid(),
            PlannedQuantity: 100,
            Priority: 1,
            PlannedStartAt: null,
            PlannedEndAt: null
        );

        ProductionOrderDto dto = await service.CreateAsync(command, CancellationToken.None);

        Assert.Equal("Draft", dto.Status); // enum mapeado para texto no DTO
        Assert.Equal("OP-0001", dto.OrderNumber); // normalizado (trim + upper) pelo service
        Assert.Single(repository.Added); // entidade foi adicionada ao repositório
        Assert.Equal(1, unitOfWork.SaveCallCount); // persistiu exatamente uma vez
    }
}
