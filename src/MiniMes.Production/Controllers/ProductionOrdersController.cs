using Microsoft.AspNetCore.Mvc;
using MiniMes.Production.Application.ProductionOrders;
using MiniMes.Production.Contracts;

namespace MiniMes.Production.Controllers;

[ApiController]
[Route("api/production-orders")]
public class ProductionOrdersController(IProductionOrderService service) : ControllerBase
{
    /// <summary>Lists every production order with its operations.</summary>
    /// <response code="200">Orders returned.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProductionOrderDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductionOrderDto>>> GetAll(
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<ProductionOrderDto> orders = await service.GetAllAsync(cancellationToken);
        return Ok(orders);
    }

    /// <summary>Gets a single production order with its operations.</summary>
    /// <param name="id">Production order identifier.</param>
    /// <response code="200">Order found.</response>
    /// <response code="404">Order not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProductionOrderDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductionOrderDto>> GetById(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        ProductionOrderDto? order = await service.GetByIdAsync(id, cancellationToken);

        if (order is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Resource not found",
                detail: $"Production order {id} not found."
            );
        }

        return Ok(order);
    }

    /// <summary>
    /// Creates a production order in <c>Draft</c> status. The order number is trimmed and
    /// upper-cased before being stored.
    /// </summary>
    /// <param name="request">Production order to create.</param>
    /// <response code="201">Order created.</response>
    /// <response code="400">Request failed validation.</response>
    /// <response code="404">Referenced product not found.</response>
    /// <response code="422">Product is inactive or the order number is already used.</response>
    [HttpPost]
    [ProducesResponseType<ProductionOrderDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
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

    /// <summary>Releases a draft order for execution: <c>Draft</c> to <c>Released</c>.</summary>
    /// <param name="id">Production order identifier.</param>
    /// <response code="204">Order released.</response>
    /// <response code="404">Order not found.</response>
    /// <response code="409">Order was modified by another process.</response>
    /// <response code="422">Order is not a draft or has no operations.</response>
    [HttpPost("{id:guid}/release")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Release(Guid id, CancellationToken cancellationToken)
    {
        await service.ReleaseAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Completes an in-progress order: <c>InProgress</c> to <c>Completed</c>.
    /// </summary>
    /// <param name="id">Production order identifier.</param>
    /// <response code="204">Order completed.</response>
    /// <response code="404">Order not found.</response>
    /// <response code="409">Order was modified by another process.</response>
    /// <response code="422">Order is not in progress or still has pending operations.</response>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
    {
        await service.CompleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>Starts a released order: <c>Released</c> to <c>InProgress</c>.</summary>
    /// <param name="id">Production order identifier.</param>
    /// <response code="204">Order started.</response>
    /// <response code="404">Order not found.</response>
    /// <response code="409">Order was modified by another process.</response>
    /// <response code="422">Order is not released.</response>
    [HttpPost("{id:guid}/start")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Start(Guid id, CancellationToken cancellationToken)
    {
        await service.StartAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>Cancels a draft or released order.</summary>
    /// <param name="id">Production order identifier.</param>
    /// <response code="204">Order cancelled.</response>
    /// <response code="404">Order not found.</response>
    /// <response code="409">Order was modified by another process.</response>
    /// <response code="422">Order is neither draft nor released.</response>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        await service.CancelAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Adds an operation to a draft order. The <c>Location</c> header points at the order:
    /// there is no individual GET for an operation.
    /// </summary>
    /// <param name="id">Production order identifier.</param>
    /// <param name="request">Operation to add.</param>
    /// <response code="201">Operation added.</response>
    /// <response code="400">Request failed validation.</response>
    /// <response code="404">Order not found.</response>
    /// <response code="409">Order was modified by another process.</response>
    /// <response code="422">Order is not a draft or the sequence is already used.</response>
    [HttpPost("{id:guid}/operations")]
    [ProducesResponseType<ProductionOperationDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ProductionOperationDto>> AddOperation(
        Guid id,
        [FromBody] AddProductionOperationRequest request,
        CancellationToken cancellationToken
    )
    {
        var command = new AddProductionOperationCommand(
            ProductionOrderId: id,
            Sequence: request.Sequence,
            Code: request.Code,
            Description: request.Description,
            WorkCenterId: request.WorkCenterId,
            PlannedQuantity: request.PlannedQuantity,
            TargetCycleTimeSeconds: request.TargetCycleTimeSeconds
        );

        ProductionOperationDto operation = await service.AddOperationAsync(
            command,
            cancellationToken
        );

        return CreatedAtAction(nameof(GetById), new { id }, operation);
    }

    /// <summary>
    /// Starts an operation of an in-progress order. Every lower sequence must already be
    /// completed or cancelled.
    /// </summary>
    /// <param name="id">Production order identifier.</param>
    /// <param name="operationId">Operation identifier.</param>
    /// <response code="204">Operation started.</response>
    /// <response code="404">Order or operation not found.</response>
    /// <response code="409">Order was modified by another process.</response>
    /// <response code="422">Order is not in progress, a previous operation is pending, or the operation is not pending.</response>
    [HttpPost("{id:guid}/operations/{operationId:guid}/start")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> StartOperation(
        Guid id,
        Guid operationId,
        CancellationToken cancellationToken
    )
    {
        await service.StartOperationAsync(id, operationId, cancellationToken);
        return NoContent();
    }

    /// <summary>Completes an operation of the order.</summary>
    /// <param name="id">Production order identifier.</param>
    /// <param name="operationId">Operation identifier.</param>
    /// <response code="204">Operation completed.</response>
    /// <response code="404">Order or operation not found.</response>
    /// <response code="409">Order was modified by another process.</response>
    /// <response code="422">Operation state does not allow completion.</response>
    [HttpPost("{id:guid}/operations/{operationId:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CompleteOperation(
        Guid id,
        Guid operationId,
        CancellationToken cancellationToken
    )
    {
        await service.CompleteOperationAsync(id, operationId, cancellationToken);
        return NoContent();
    }

    /// <summary>Cancels an operation of the order.</summary>
    /// <param name="id">Production order identifier.</param>
    /// <param name="operationId">Operation identifier.</param>
    /// <response code="204">Operation cancelled.</response>
    /// <response code="404">Order or operation not found.</response>
    /// <response code="409">Order was modified by another process.</response>
    /// <response code="422">Operation state does not allow cancellation.</response>
    [HttpPost("{id:guid}/operations/{operationId:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CancelOperation(
        Guid id,
        Guid operationId,
        CancellationToken cancellationToken
    )
    {
        await service.CancelOperationAsync(id, operationId, cancellationToken);
        return NoContent();
    }
}
