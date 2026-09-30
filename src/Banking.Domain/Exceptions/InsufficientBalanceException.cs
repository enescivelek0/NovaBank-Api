namespace Banking.Domain.Exceptions;

public class InsufficientBalanceException : DomainException
{
    public decimal CurrentBalance { get; }
    public decimal RequestedAmount { get; }

    public InsufficientBalanceException(decimal currentBalance, decimal requestedAmount)
        : base($"İşlem için yetersiz bakiye! Mevcut Bakiye: {currentBalance:N2}, Çekilmek/Transfer edilmek istenen: {requestedAmount:N2}")
    {
        CurrentBalance = currentBalance;
        RequestedAmount = requestedAmount;
    }
}
