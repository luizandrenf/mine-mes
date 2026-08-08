using Microsoft.AspNetCore.Mvc;
using MiniMes.Production.Application.Products;
using MiniMes.Production.Contracts;

namespace MiniMes.Production.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(IProductService service) : ControllerBase
{
    /// <summary>Lists every product.</summary>
    /// <response code="200">Products returned.</response>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProductDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll(
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<ProductDto> products = await service.GetAllAsync(cancellationToken);
        return Ok(products);
    }

    /// <summary>Gets a single product by its identifier.</summary>
    /// <param name="id">Product identifier.</param>
    /// <response code="200">Product found.</response>
    /// <response code="404">Product not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProductDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        ProductDto? product = await service.GetByIdAsync(id, cancellationToken);

        if (product is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Resource not found",
                detail: $"Product {id} not found."
            );
        }

        return Ok(product);
    }

    /// <summary>Creates a product. The code is trimmed and upper-cased before being stored.</summary>
    /// <param name="request">Product to create.</param>
    /// <response code="201">Product created.</response>
    /// <response code="400">Request failed validation.</response>
    /// <response code="422">A product with the same code already exists.</response>
    [HttpPost]
    [ProducesResponseType<ProductDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken
    )
    {
        var command = new CreateProductCommand(Code: request.Code, Name: request.Name);

        ProductDto product = await service.CreateAsync(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    /// <summary>Activates a product, making it usable in new production orders. Idempotent.</summary>
    /// <param name="id">Product identifier.</param>
    /// <response code="204">Product activated.</response>
    /// <response code="404">Product not found.</response>
    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        await service.ActivateAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>Deactivates a product, blocking it from new production orders. Idempotent.</summary>
    /// <param name="id">Product identifier.</param>
    /// <response code="204">Product deactivated.</response>
    /// <response code="404">Product not found.</response>
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await service.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }
}
