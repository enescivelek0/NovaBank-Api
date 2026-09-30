using Banking.Domain.Entities;

namespace Banking.Application.Common.Interfaces;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Account?> GetByIbanAsync(string iban, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Account>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task AddAsync(Account account, CancellationToken cancellationToken = default);
    void Update(Account account);
}
