using Banking.Application.Common.Interfaces;
using Banking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Banking.Infrastructure.Persistence.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly BankingDbContext _context;

    public AccountRepository(BankingDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .Include(a => a.Customer)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Account?> GetByIbanAsync(string iban, CancellationToken cancellationToken = default)
    {
        var sanitized = iban.Replace(" ", string.Empty).ToUpperInvariant();
        return await _context.Accounts
            .Include(a => a.Customer)
            .FirstOrDefaultAsync(a => a.Iban == sanitized, cancellationToken);
    }

    public async Task<IReadOnlyList<Account>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .Where(a => a.CustomerId == customerId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Account account, CancellationToken cancellationToken = default)
    {
        await _context.Accounts.AddAsync(account, cancellationToken);
    }

    public void Update(Account account)
    {
        _context.Accounts.Update(account);
    }
}
