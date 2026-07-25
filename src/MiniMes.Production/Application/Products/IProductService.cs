namespace MiniMes.Production.Application.Products;

public interface IProductService
{
    Task<ProductDto> CreateAsync(CreateProductCommand command, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task ActivateAsync(Guid id, CancellationToken cancellationToken);

    Task DeactivateAsync(Guid id, CancellationToken cancellationToken);
}
