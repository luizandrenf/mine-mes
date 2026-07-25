using Microsoft.AspNetCore.Mvc;
using MiniMes.Production.Application.ProductionOrders;
using MiniMes.Production.Contracts;

namespace MiniMes.Production.Controllers;

[ApiController]
[Route("api/production-orders")]
public class ProductionOrdersController(IProductionOrderService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductionOrderDto>>> GetAll(
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<ProductionOrderDto> orders = await service.GetAllAsync(cancellationToken);
        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductionOrderDto>> GetById(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        ProductionOrderDto? order = await service.GetByIdAsync(id, cancellationToken);

        if (order is null)
        {
            return NotFound(new { message = $"Production order {id} not found." });
        }

        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<ProductionOrderDto>> Create(
        [FromBody] CreateProductionOrderRequest request,
        CancellationToken cancellationToken
    )
    {
        var command = new CreateProductionOrderCommand(
            OrderNumber: request.OrderNumber,
            ProductId: request.ProductId,
            PlannedQuantity: request.PlannedQuantity,
            Priority: request.Priority,
            PlannedStartAt: request.PlannedStartAt,
            PlannedEndAt: request.PlannedEndAt
        );

        ProductionOrderDto order = await service.CreateAsync(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }
}
