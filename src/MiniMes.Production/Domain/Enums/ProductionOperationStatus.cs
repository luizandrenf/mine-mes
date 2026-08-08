namespace MiniMes.Production.Domain.Enums;

// ponytail: Ready=2 and Paused=4 from the spec are left out — nothing in Production drives them.
// Numbering is preserved so they can be added without a data migration once Execution exists.
public enum ProductionOperationStatus
{
    Pending = 1,
    InProgress = 3,
    Completed = 5,
    Cancelled = 6,
}
