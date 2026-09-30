using Banking.Domain.Common;
using Banking.Domain.Enums;
using Banking.Domain.Exceptions;
using Banking.Domain.ValueObjects;

namespace Banking.Domain.Entities;

public class Account : BaseEntity
{
    public Guid CustomerId { get; private set; }
    public Customer? Customer { get; private set; }
    public string Iban { get; private set; } = null!;
    public decimal Balance { get; private set; }
    public Currency Currency { get; private set; }
    public bool IsActive { get; private set; }

    protected Account() { } // EF Core

    public Account(Guid customerId, Currency currency, decimal initialBalance = 0m, string? customIban = null)
    {
        if (customerId == Guid.Empty)
            throw new InvalidAccountOperationException("Hesap için geçerli bir CustomerId belirtilmelidir.");

        if (initialBalance < 0)
            throw new InsufficientBalanceException(0, initialBalance);

        CustomerId = customerId;
        Currency = currency;
        Balance = Math.Round(initialBalance, 2, MidpointRounding.AwayFromZero);
        IsActive = true;

        Iban = string.IsNullOrWhiteSpace(customIban)
            ? ValueObjects.Iban.GenerateRandom().Value
            : ValueObjects.Iban.Create(customIban).Value;
    }

    public void Deposit(decimal amount)
    {
        EnsureActive();

        if (amount <= 0)
            throw new InvalidAccountOperationException("Yatırılacak tutar sıfırdan büyük olmalıdır.");

        var roundedAmount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        Balance += roundedAmount;
    }

    public void Withdraw(decimal amount)
    {
        EnsureActive();

        if (amount <= 0)
            throw new InvalidAccountOperationException("Çekilecek tutar sıfırdan büyük olmalıdır.");

        var roundedAmount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);

        if (Balance < roundedAmount)
            throw new InsufficientBalanceException(Balance, roundedAmount);

        Balance -= roundedAmount;
    }

    public void TransferTo(Account targetAccount, decimal amount)
    {
        EnsureActive();
        ArgumentNullException.ThrowIfNull(targetAccount);

        if (targetAccount.Id == Id)
            throw new InvalidAccountOperationException("Aynı hesaplar arasında transfer yapılamaz.");

        if (!targetAccount.IsActive)
            throw new InvalidAccountOperationException("Hedef hesap aktif değil.");

        if (Currency != targetAccount.Currency)
            throw new InvalidAccountOperationException($"Hesap para birimleri uyuşmuyor: {Currency} -> {targetAccount.Currency}");

        Withdraw(amount);
        targetAccount.Deposit(amount);
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    private void EnsureActive()
    {
        if (!IsActive)
            throw new InvalidAccountOperationException($"Hesap (Id: {Id}) aktif durumda değil.");
    }
}
