using Banking.Domain.Enums;
using Banking.Domain.Exceptions;

namespace Banking.Domain.ValueObjects;

public sealed record Money
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    public Money(decimal amount, Currency currency)
    {
        if (amount < 0)
        {
            throw new InvalidAccountOperationException("Para tutarı negatif olamaz.");
        }

        Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = currency;
    }

    public static Money Zero(Currency currency) => new(0m, currency);

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        if (Amount < other.Amount)
        {
            throw new InsufficientBalanceException(Amount, other.Amount);
        }
        return new Money(Amount - other.Amount, Currency);
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new InvalidAccountOperationException(
                $"Farklı para birimleri arasında işlem yapılamaz: {Currency} ve {other.Currency}");
        }
    }

    public override string ToString() => $"{Amount:N2} {Currency}";
}
