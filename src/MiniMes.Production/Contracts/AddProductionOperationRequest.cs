using System.ComponentModel.DataAnnotations;

namespace MiniMes.Production.Contracts;

public class AddProductionOperationRequest
{
    [Range(1, int.MaxValue)]
    public int Sequence { get; set; }

    [Required]
    [MinLength(1)]
    [MaxLength(50)]
    public required string Code { get; set; }

    [Required]
    [MinLength(1)]
    [MaxLength(200)]
    public required string Description { get; set; }

    [Required]
    public Guid WorkCenterId { get; set; }

    [Range(0.001, 1_000_000)]
    public decimal PlannedQuantity { get; set; }

    [Range(1, int.MaxValue)]
    public int? TargetCycleTimeSeconds { get; set; }
}
