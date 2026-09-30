using System.Text.RegularExpressions;
using Banking.Domain.Exceptions;

namespace Banking.Domain.ValueObjects;

public sealed record Iban
{
    private static readonly Regex IbanRegex = new(@"^TR\d{24}$", RegexOptions.Compiled);

    public string Value { get; }

    private Iban(string value)
    {
        Value = value;
    }

    public static Iban Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidIbanException(value, "IBAN değeri boş olamaz.");
        }

        var sanitized = value.Replace(" ", string.Empty).Trim().ToUpperInvariant();

        if (sanitized.Length != 26)
        {
            throw new InvalidIbanException(sanitized, $"TR IBAN uzunluğu 26 karakter olmalıdır. Mevcut: {sanitized.Length}");
        }

        if (!IbanRegex.IsMatch(sanitized))
        {
            throw new InvalidIbanException(sanitized, "IBAN 'TR' ile başlamalı ve ardından 24 rakam içermelidir.");
        }

        return new Iban(sanitized);
    }

    public static Iban GenerateRandom(string bankCode = "00061")
    {
        // 00061 is typically Ziraat / standard bank code
        var paddedBankCode = bankCode.PadLeft(5, '0')[..5];
        var random = Random.Shared;
        var checkDigits = random.Next(10, 99).ToString();
        var reserved = "0";
        var accountNumber = random.NextInt64(1000000000000000, 9999999999999999).ToString();

        var ibanString = $"TR{checkDigits}{paddedBankCode}{reserved}{accountNumber}";
        return new Iban(ibanString);
    }

    public string Formatted =>
        $"{Value[..4]} {Value.Substring(4, 4)} {Value.Substring(8, 4)} {Value.Substring(12, 4)} {Value.Substring(16, 4)} {Value.Substring(20, 4)} {Value.Substring(24, 2)}";

    public override string ToString() => Value;

    public static implicit operator string(Iban iban) => iban.Value;
}
