using MiniMes.Production.Domain.Exceptions;

namespace MiniMes.Production.Domain.Entities;

public class Product
{
    // EF Core materialization only.
    private Product() { }

    public Product(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException("Product code is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Product name is required.");
        }

        Id = Guid.NewGuid();
        Code = code;
        Name = name;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public string Code { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
