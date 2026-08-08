using System.ComponentModel.DataAnnotations;

namespace MiniMes.Production.Contracts;

public class AddProductionOperationRequest
{
    /// <summary>Execution order inside the production order. Unique within the order.</summary>
    [Range(1, int.MaxValue)]
    public int Sequence { get; set; }

    /// <summary>Operation code. Stored trimmed and upper-cased.</summary>
    [Required]
    [MinLength(1)]
    [MaxLength(50)]
    public required string Code { get; set; }

    /// <summary>What the operation does.</summary>
    [Required]
    [MinLength(1)]
    [MaxLength(200)]
    public required string Description { get; set; }

    /// <summary>Work center that runs the operation. Owned by the Equipment service, not validated here.</summary>
    [Required]
    public Guid WorkCenterId { get; set; }

    /// <summary>Quantity planned for this operation.</summary>
    [Range(0.001, 1_000_000)]
    public decimal PlannedQuantity { get; set; }

    /// <summary>Target cycle time per unit, in seconds.</summary>
    [Range(1, int.MaxValue)]
    public int? TargetCycleTimeSeconds { get; set; }
}
