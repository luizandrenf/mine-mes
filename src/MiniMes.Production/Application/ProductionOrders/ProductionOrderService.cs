using MiniMes.Production.Application.Abstractions;
using MiniMes.Production.Application.Products;
using MiniMes.Production.Domain.Entities;
using MiniMes.Production.Domain.Exceptions;

namespace MiniMes.Production.Application.ProductionOrders;

public sealed class ProductionOrderService(
    IProductionOrderRepository repository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork
) : IProductionOrderService
{
    public async Task<ProductionOrderDto> CreateAsync(
        CreateProductionOrderCommand command,
        CancellationToken cancellationToken
    )
    {
        Product? product = await productRepository.GetByIdAsync(
            command.ProductId,
            cancellationToken
        );

        if (product is null)
        {
            throw new NotFoundException($"Product {command.ProductId} not found.");
        }

        if (!product.IsActive)
        {
            throw new DomainException(
                $"Product {product.Code} is inactive and cannot be used in a new order."
            );
        }

        string orderNumber = command.OrderNumber.Trim().ToUpperInvariant();

        if (await repository.OrderNumberExistsAsync(orderNumber, cancellationToken))
        {
            throw new DomainException(
                $"A production order with number '{orderNumber}' already exists."
            );
        }

        var order = new ProductionOrder(
            orderNumber: orderNumber,
            productId: command.ProductId,
            plannedQuantity: command.PlannedQuantity,
            priority: command.Priority,
            plannedStartAt: command.PlannedStartAt,
            plannedEndAt: command.PlannedEndAt
        );

        repository.Add(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ProductionOrderDto.From(order);
    }

    public async Task<IReadOnlyList<ProductionOrderDto>> GetAllAsync(
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<ProductionOrder> orders = await repository.GetAllAsync(cancellationToken);
        return orders.Select(ProductionOrderDto.From).ToList();
    }

    public async Task<ProductionOrderDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        ProductionOrder? order = await repository.GetByIdAsync(id, cancellationToken);
        return order is null ? null : ProductionOrderDto.From(order);
    }

    private async Task TransitionAsync(
        Guid id,
        Action<ProductionOrder> transition,
        CancellationToken cancellationToken
    )
    {
        ProductionOrder? order = await repository.GetByIdForUpdateAsync(id, cancellationToken);

        if (order is null)
        {
            throw new NotFoundException($"Production order {id} not found.");
        }

        transition(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public Task ReleaseAsync(Guid id, CancellationToken cancellationToken) =>
        TransitionAsync(id, order => order.Release(), cancellationToken);

    public Task StartAsync(Guid id, CancellationToken cancellationToken) =>
        TransitionAsync(id, order => order.Start(), cancellationToken);

    public Task CompleteAsync(Guid id, CancellationToken cancellationToken) =>
        TransitionAsync(id, order => order.Complete(), cancellationToken);

    public Task CancelAsync(Guid id, CancellationToken cancellationToken) =>
        TransitionAsync(id, order => order.Cancel(), cancellationToken);
}
