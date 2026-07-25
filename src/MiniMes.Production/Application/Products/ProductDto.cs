using MiniMes.Production.Domain.Entities;

namespace MiniMes.Production.Application.Products;

public sealed record ProductDto(
    Guid Id,
    string Code,
    string Name,
    bool IsActive,
    DateTime CreatedAt
)
{
    public static ProductDto From(Product product) =>
        new(product.Id, product.Code, product.Name, product.IsActive, product.CreatedAt);
}
