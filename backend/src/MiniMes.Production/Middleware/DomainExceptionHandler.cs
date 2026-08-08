using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMes.Production.Domain.Exceptions;

namespace MiniMes.Production.Middleware;

public sealed class DomainExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<DomainExceptionHandler> logger
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        (int status, string title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            DomainException => (StatusCodes.Status422UnprocessableEntity, "Business rule violated"),
            DbUpdateConcurrencyException => (
                StatusCodes.Status409Conflict,
                "Resource was modified by another process"
            ),
            _ => (StatusCodes.Status500InternalServerError, "Internal error"),
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception");
        }

        httpContext.Response.StatusCode = status;

        return await problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Status = status,
                    Title = title,
                    Detail =
                        status == StatusCodes.Status500InternalServerError
                            ? "An unexpected error occurred."
                            : exception.Message,
                },
            }
        );
    }
}
