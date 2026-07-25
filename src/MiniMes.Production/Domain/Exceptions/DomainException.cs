namespace MiniMes.Production.Domain.Exceptions;

// Domain invariant/transition violation. Mapped to HTTP 422 by DomainExceptionHandler.
public sealed class DomainException(string message) : Exception(message);
