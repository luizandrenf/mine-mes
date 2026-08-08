using System.ComponentModel.DataAnnotations;

namespace MiniMes.Production.Contracts;

public class CreateProductRequest
{
    /// <summary>Unique product code. Stored trimmed and upper-cased.</summary>
    [Required]
    [MinLength(1)]
    [MaxLength(50)]
    public required string Code { get; set; }

    /// <summary>Product display name.</summary>
    [Required]
    [MinLength(1)]
    [MaxLength(200)]
    public required string Name { get; set; }
}
