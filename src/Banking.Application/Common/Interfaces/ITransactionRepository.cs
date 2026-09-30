using Banking.Domain.Entities;

namespace Banking.Application.Common.Interfaces;

public interface ITransactionRepository
{
    Task<IReadOnlyList<Transaction>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
}
