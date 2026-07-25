using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMes.Api.Contracts;
using MiniMes.Api.Data;
using MiniMes.Api.Models;

namespace MiniMes.api.Controllers;

[ApiController]
[Route("api/production-orders")]
public class ProductionOrdersController : ControllerBase
{
    private readonly MiniMesDbContext _dbContext;

    public ProductionOrdersController(MiniMesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductionOrder>>> GetAll(
        CancellationToken cancellationToken
    )
    {
        List<ProductionOrder> orders = await _dbContext
            .ProductionOrders.AsNoTracking()
            .OrderByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductionOrder>> GetById(
        int id,
        CancellationToken cancellationToken
    )
    {
        ProductionOrder? order = await _dbContext.ProductionOrders.FirstOrDefaultAsync(
            order => order.Id == id,
            cancellationToken
        );

        if (order is null)
        {
            return NotFound(new { message = $"Ordem de produção {id} não encontrada." });
        }

        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<ProductionOrder>> Create(
        [FromBody] CreateProductionOrderRequest request,
        CancellationToken cancellationToken
    )
    {
        var order = new ProductionOrder
        {
            ProductCode = request.ProductCode.Trim().ToUpperInvariant(),
            PlannedQuantity = request.PlannedQuantity,
        };

        _dbContext.ProductionOrders.Add(order);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }
}
