namespace MiniMes.Api.Domain.Exceptions;

// Lançada quando uma invariante ou transição de estado do domínio é violada.
// Num passo futuro será mapeada para HTTP 422/409 por um handler global.
public sealed class DomainException : Exception
{
    public DomainException(string message)
        : base(message) { }
}
