using System.ComponentModel.DataAnnotations;

namespace MiniMes.Production.Contracts;

public class CreateProductRequest
{
    [Required]
    [MinLength(1)]
    [MaxLength(50)]
    public required string Code { get; set; }

    [Required]
    [MinLength(1)]
    [MaxLength(200)]
    public required string Name { get; set; }
}
