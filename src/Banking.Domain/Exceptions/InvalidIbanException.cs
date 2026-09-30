namespace Banking.Domain.Exceptions;

public class InvalidIbanException : DomainException
{
    public InvalidIbanException(string iban, string reason)
        : base($"Geçersiz IBAN '{iban}': {reason}")
    {
    }
}
