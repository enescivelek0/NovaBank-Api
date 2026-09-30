using Banking.Application.Common.Interfaces;
using Banking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Banking.Infrastructure.Persistence.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly BankingDbContext _context;

    public TransactionRepository(BankingDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Transaction>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Where(t => t.SourceAccountId == accountId || t.TargetAccountId == accountId)
            .OrderByDescending(t => t.TransactionDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        await _context.Transactions.AddAsync(transaction, cancellationToken);
    }
}
