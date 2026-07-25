using MiniMes.Api.Domain.Enums;
using MiniMes.Api.Domain.Exceptions;

namespace MiniMes.Api.Domain.Entities;

public class ProductionOrder
{
    // Construtor sem parâmetros usado APENAS pelo EF Core ao materializar do banco.
    // É private para o resto da aplicação não conseguir criar uma ordem em estado inválido.
    private ProductionOrder() { }

    public ProductionOrder(
        string orderNumber,
        Guid productId,
        decimal plannedQuantity,
        int priority,
        DateTime? plannedStartAt = null,
        DateTime? plannedEndAt = null
    )
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            throw new DomainException("O número da ordem é obrigatório.");
        }

        if (plannedQuantity <= 0)
        {
            throw new DomainException("A quantidade planejada deve ser maior que zero.");
        }

        Id = Guid.NewGuid();
        OrderNumber = orderNumber;
        ProductId = productId;
        PlannedQuantity = plannedQuantity;
        Priority = priority;
        PlannedStartAt = plannedStartAt;
        PlannedEndAt = plannedEndAt;
        Status = ProductionOrderStatus.Draft;
        CreatedAt = DateTime.UtcNow;
        Version = Guid.NewGuid();
    }

    public Guid Id { get; private set; }

    public string OrderNumber { get; private set; } = null!;

    // ponytail: hoje é só um Guid solto; vira FK para products quando a entidade Product existir.
    public Guid ProductId { get; private set; }

    public decimal PlannedQuantity { get; private set; }

    public ProductionOrderStatus Status { get; private set; }

    public int Priority { get; private set; }

    public DateTime? PlannedStartAt { get; private set; }

    public DateTime? PlannedEndAt { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? ReleasedAt { get; private set; }

    // Token de concorrência otimista: muda a cada transição para o banco detectar escritas concorrentes.
    public Guid Version { get; private set; }

    public void Release()
    {
        if (Status != ProductionOrderStatus.Draft)
        {
            throw new DomainException(
                $"Só é possível liberar uma ordem em rascunho. Estado atual: {Status}."
            );
        }

        // ponytail: exigir pelo menos uma operação quando ProductionOperation existir.
        Status = ProductionOrderStatus.Released;
        ReleasedAt = DateTime.UtcNow;
        Version = Guid.NewGuid();
    }

    public void Start()
    {
        if (Status != ProductionOrderStatus.Released)
        {
            throw new DomainException(
                $"Só é possível iniciar uma ordem liberada. Estado atual: {Status}."
            );
        }

        Status = ProductionOrderStatus.InProgress;
        Version = Guid.NewGuid();
    }

    public void Complete()
    {
        if (Status != ProductionOrderStatus.InProgress)
        {
            throw new DomainException(
                $"Só é possível concluir uma ordem em andamento. Estado atual: {Status}."
            );
        }

        // ponytail: exigir que todas as operações estejam concluídas quando existirem.
        Status = ProductionOrderStatus.Completed;
        Version = Guid.NewGuid();
    }

    public void Cancel()
    {
        if (Status != ProductionOrderStatus.Draft && Status != ProductionOrderStatus.Released)
        {
            throw new DomainException(
                $"Só é possível cancelar uma ordem em rascunho ou liberada. Estado atual: {Status}."
            );
        }

        Status = ProductionOrderStatus.Cancelled;
        Version = Guid.NewGuid();
    }
}
