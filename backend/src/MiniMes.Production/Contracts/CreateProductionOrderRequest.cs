using System.ComponentModel.DataAnnotations;

namespace MiniMes.Production.Contracts;

public class CreateProductionOrderRequest
{
    /// <summary>Unique order number. Stored trimmed and upper-cased.</summary>
    [Required]
    [MinLength(2)]
    [MaxLength(50)]
    public required string OrderNumber { get; set; }

    /// <summary>Product to be manufactured. Must exist and be active.</summary>
    [Required]
    public Guid ProductId { get; set; }

    /// <summary>Quantity to produce.</summary>
    [Range(0.001, 1_000_000)]
    public decimal PlannedQuantity { get; set; }

    /// <summary>Scheduling priority. Higher means more urgent.</summary>
    [Range(0, int.MaxValue)]
    public int Priority { get; set; }

    /// <summary>Planned start, in UTC. Informational only.</summary>
    public DateTime? PlannedStartAt { get; set; }

    /// <summary>Planned end, in UTC. Informational only.</summary>
    public DateTime? PlannedEndAt { get; set; }
}
