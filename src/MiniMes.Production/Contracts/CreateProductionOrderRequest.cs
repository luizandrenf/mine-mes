using System.ComponentModel.DataAnnotations;

namespace MiniMes.Production.Contracts;

public class CreateProductionOrderRequest
{
    [Required]
    [MinLength(2)]
    [MaxLength(50)]
    public required string OrderNumber { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    [Range(0.001, 1_000_000)]
    public decimal PlannedQuantity { get; set; }

    [Range(0, int.MaxValue)]
    public int Priority { get; set; }

    public DateTime? PlannedStartAt { get; set; }

    public DateTime? PlannedEndAt { get; set; }
}
