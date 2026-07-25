namespace MiniMes.Api.Application.Abstractions;

// Representa a confirmação das alterações. Um único SaveChangesAsync já é uma transação.
// O service decide QUANDO chamar; os repositories deixam de ter SaveChanges próprio.
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
