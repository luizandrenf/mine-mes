using MiniMes.Production.Application.Abstractions;
using MiniMes.Production.Domain.Entities;
using MiniMes.Production.Domain.Exceptions;

namespace MiniMes.Production.Application.Products;

public sealed class ProductService(IProductRepository repository, IUnitOfWork unitOfWork)
    : IProductService
{
    public async Task<ProductDto> CreateAsync(
        CreateProductCommand command,
        CancellationToken cancellationToken
    )
    {
        string code = command.Code.Trim().ToUpperInvariant();

        if (await repository.CodeExistsAsync(code, cancellationToken))
        {
            throw new DomainException($"A product with code '{code}' already exists.");
        }

        var product = new Product(code, command.Name.Trim());

        repository.Add(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ProductDto.From(product);
    }

    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<Product> products = await repository.GetAllAsync(cancellationToken);
        return products.Select(ProductDto.From).ToList();
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        Product? product = await repository.GetByIdAsync(id, cancellationToken);
        return product is null ? null : ProductDto.From(product);
    }

    public Task ActivateAsync(Guid id, CancellationToken cancellationToken) =>
        ChangeActiveAsync(id, activate: true, cancellationToken);

    public Task DeactivateAsync(Guid id, CancellationToken cancellationToken) =>
        ChangeActiveAsync(id, activate: false, cancellationToken);

    private async Task ChangeActiveAsync(
        Guid id,
        bool activate,
        CancellationToken cancellationToken
    )
    {
        Product? product = await repository.GetByIdForUpdateAsync(id, cancellationToken);

        if (product is null)
        {
            throw new NotFoundException($"Product {id} not found.");
        }

        if (activate)
        {
            product.Activate();
        }
        else
        {
            product.Deactivate();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
