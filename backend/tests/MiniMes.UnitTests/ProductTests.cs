using MiniMes.Production.Domain.Entities;
using MiniMes.Production.Domain.Exceptions;
using Xunit;

namespace MiniMes.UnitTests;

public class ProductTests
{
    [Fact]
    public void New_product_is_active()
    {
        var product = new Product("P-1", "Motor");

        Assert.True(product.IsActive);
        Assert.NotEqual(Guid.Empty, product.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_rejects_blank_code(string code)
    {
        Assert.Throws<DomainException>(() => new Product(code, "Motor"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_rejects_blank_name(string name)
    {
        Assert.Throws<DomainException>(() => new Product("P-1", name));
    }

    [Fact]
    public void Deactivate_then_activate_toggles_flag()
    {
        var product = new Product("P-1", "Motor");

        product.Deactivate();
        Assert.False(product.IsActive);

        product.Activate();
        Assert.True(product.IsActive);
    }
}
