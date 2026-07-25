using Microsoft.AspNetCore.Mvc;
using MiniMes.Api.Application.ProductionOrders;
using MiniMes.Api.Contracts;

namespace MiniMes.Api.Controllers;

[ApiController]
[Route("api/production-orders")]
public class ProductionOrdersController : ControllerBase
{
    private readonly IProductionOrderService _service;

    public ProductionOrdersController(IProductionOrderService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductionOrderDto>>> GetAll(
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<ProductionOrderDto> orders = await _service.GetAllAsync(cancellationToken);
        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductionOrderDto>> GetById(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        ProductionOrderDto? order = await _service.GetByIdAsync(id, cancellationToken);

        if (order is null)
        {
            return NotFound(new { message = $"Ordem de produção {id} não encontrada." });
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

        ProductionOrderDto order = await _service.CreateAsync(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }
}
