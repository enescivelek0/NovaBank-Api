using Banking.Domain.Entities;

namespace Banking.Application.Common.Interfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Customer?> GetByIdentityNumberAsync(string identityNumber, CancellationToken cancellationToken = default);
    Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Customer>> ListAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
}
