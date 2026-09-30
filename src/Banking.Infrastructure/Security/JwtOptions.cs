namespace Banking.Infrastructure.Security;

public class JwtOptions
{
    public const string SectionName = "JwtSettings";

    public string Issuer { get; set; } = "BankingApi";
    public string Audience { get; set; } = "BankingApiClients";
    public string SecretKey { get; set; } = "SuperSecretBankingKey_MustBeAtLeast32BytesLong!";
    public int ExpiryHours { get; set; } = 4;
}
