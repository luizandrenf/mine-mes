namespace MiniMes.Production.Domain.Exceptions;

// Referenced resource does not exist. Mapped to HTTP 404 by DomainExceptionHandler.
public sealed class NotFoundException(string message) : Exception(message);
