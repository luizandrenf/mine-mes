using MiniMes.Api.Application.Abstractions;
using MiniMes.Api.Domain.Entities;

namespace MiniMes.Api.Application.ProductionOrders;

public sealed class ProductionOrderService : IProductionOrderService
{
    private readonly IProductionOrderRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ProductionOrderService(
        IProductionOrderRepository repository,
        IUnitOfWork unitOfWork
    )
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductionOrderDto> CreateAsync(
        CreateProductionOrderCommand command,
        CancellationToken cancellationToken
    )
    {
        var order = new ProductionOrder(
            orderNumber: command.OrderNumber.Trim().ToUpperInvariant(),
            productId: command.ProductId,
            plannedQuantity: command.PlannedQuantity,
            priority: command.Priority,
            plannedStartAt: command.PlannedStartAt,
            plannedEndAt: command.PlannedEndAt
        );

        _repository.Add(order);

        // O service decide o momento de persistir. Um SaveChangesAsync já é transacional.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ProductionOrderDto.From(order);
    }

    public async Task<IReadOnlyList<ProductionOrderDto>> GetAllAsync(
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<ProductionOrder> orders = await _repository.GetAllAsync(cancellationToken);
        return orders.Select(ProductionOrderDto.From).ToList();
    }

    public async Task<ProductionOrderDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        ProductionOrder? order = await _repository.GetByIdAsync(id, cancellationToken);
        return order is null ? null : ProductionOrderDto.From(order);
    }
}
