using Banking.Domain.Enums;

namespace Banking.Application.Common.DTOs;

public record CustomerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string IdentityNumber,
    DateTime CreatedAt
);

public record AccountDto(
    Guid Id,
    Guid CustomerId,
    string Iban,
    decimal Balance,
    Currency Currency,
    bool IsActive,
    DateTime CreatedAt
);

public record AccountBalanceDto(
    Guid AccountId,
    string Iban,
    decimal Balance,
    Currency Currency,
    bool IsActive
);

public record TransactionDto(
    Guid Id,
    Guid? SourceAccountId,
    Guid? TargetAccountId,
    decimal Amount,
    Currency Currency,
    TransactionType TransactionType,
    DateTime TransactionDate,
    string Description
);

public record AuthResponseDto(
    string Token,
    Guid CustomerId,
    string FullName,
    string Email,
    DateTime ExpiresAt
);
