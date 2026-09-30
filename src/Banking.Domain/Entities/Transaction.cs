using Banking.Domain.Common;
using Banking.Domain.Enums;
using Banking.Domain.Exceptions;

namespace Banking.Domain.Entities;

public class Transaction : BaseEntity
{
    public Guid? SourceAccountId { get; private set; }
    public Account? SourceAccount { get; private set; }

    public Guid? TargetAccountId { get; private set; }
    public Account? TargetAccount { get; private set; }

    public decimal Amount { get; private set; }
    public Currency Currency { get; private set; }
    public TransactionType TransactionType { get; private set; }
    public DateTime TransactionDate { get; private set; } = DateTime.UtcNow;
    public string Description { get; private set; } = null!;

    protected Transaction() { } // EF Core

    public static Transaction CreateDeposit(Guid accountId, decimal amount, Currency currency, string? description = null)
    {
        if (amount <= 0)
            throw new InvalidAccountOperationException("Yatırma tutarı sıfırdan büyük olmalıdır.");

        return new Transaction
        {
            TargetAccountId = accountId,
            Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero),
            Currency = currency,
            TransactionType = TransactionType.Deposit,
            Description = description ?? $"Para Yatırma: {amount:N2} {currency}"
        };
    }

    public static Transaction CreateWithdrawal(Guid accountId, decimal amount, Currency currency, string? description = null)
    {
        if (amount <= 0)
            throw new InvalidAccountOperationException("Çekme tutarı sıfırdan büyük olmalıdır.");

        return new Transaction
        {
            SourceAccountId = accountId,
            Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero),
            Currency = currency,
            TransactionType = TransactionType.Withdrawal,
            Description = description ?? $"Para Çekme: {amount:N2} {currency}"
        };
    }

    public static Transaction CreateTransfer(Guid sourceAccountId, Guid targetAccountId, decimal amount, Currency currency, string? description = null)
    {
        if (amount <= 0)
            throw new InvalidAccountOperationException("Transfer tutarı sıfırdan büyük olmalıdır.");

        if (sourceAccountId == targetAccountId)
            throw new InvalidAccountOperationException("Aynı hesaplar arasında transfer yapılamaz.");

        return new Transaction
        {
            SourceAccountId = sourceAccountId,
            TargetAccountId = targetAccountId,
            Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero),
            Currency = currency,
            TransactionType = TransactionType.Transfer,
            Description = description ?? $"Hesaplar Arası Havale/EFT: {amount:N2} {currency}"
        };
    }
}
