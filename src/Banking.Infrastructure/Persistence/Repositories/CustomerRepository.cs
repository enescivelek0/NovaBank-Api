using Banking.Application.Common.Interfaces;
using Banking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Banking.Infrastructure.Persistence.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly BankingDbContext _context;

    public CustomerRepository(BankingDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .Include(c => c.Accounts)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Customer?> GetByIdentityNumberAsync(string identityNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .Include(c => c.Accounts)
            .FirstOrDefaultAsync(c => c.IdentityNumber == identityNumber, cancellationToken);
    }

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .Include(c => c.Accounts)
            .FirstOrDefaultAsync(c => c.Email == email.ToLower(), cancellationToken);
    }

    public async Task<IReadOnlyList<Customer>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .Include(c => c.Accounts)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        await _context.Customers.AddAsync(customer, cancellationToken);
    }
}
