using Banking.Domain.Common;
using Banking.Domain.Exceptions;

namespace Banking.Domain.Entities;

public class Customer : BaseEntity
{
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string IdentityNumber { get; private set; } = null!; // TCKN / VKN

    private readonly List<Account> _accounts = [];
    public IReadOnlyCollection<Account> Accounts => _accounts.AsReadOnly();

    protected Customer() { } // EF Core

    public Customer(string firstName, string lastName, string email, string identityNumber)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new InvalidAccountOperationException("Müşteri adı boş olamaz.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new InvalidAccountOperationException("Müşteri soyadı boş olamaz.");

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new InvalidAccountOperationException("Geçerli bir e-posta adresi girilmelidir.");

        if (string.IsNullOrWhiteSpace(identityNumber) || identityNumber.Length is < 10 or > 11)
            throw new InvalidAccountOperationException("Kimlik numarası (TCKN 11 hane veya VKN 10 hane) geçerli uzunlukta olmalıdır.");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim().ToLowerInvariant();
        IdentityNumber = identityNumber.Trim();
    }

    public string FullName => $"{FirstName} {LastName}";

    public void AddAccount(Account account)
    {
        ArgumentNullException.ThrowIfNull(account);
        _accounts.Add(account);
    }
}
