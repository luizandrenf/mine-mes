namespace MiniMes.Api.Models;

public class ProductionOrder
{
    public int Id { get; set; }

    public required string ProductCode { get; set; }

    public int PlannedQuantity { get; set; }

    public string Status { get; private set; } = "Created";

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
}