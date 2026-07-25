using System.ComponentModel.DataAnnotations;

namespace MiniMes.Api.Contracts;


public class CreateProductionOrderRequest
{
    [Required]
    [MinLength(2)]
    public required string ProductCode {get; set;}

    [Range(1, 1_000_000)]
    public int PlannedQuantity {get; set;}

}
