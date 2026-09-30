using Banking.Application.Common.DTOs;
using Banking.Application.Common.Interfaces;
using Banking.Domain.Entities;
using Banking.Domain.Exceptions;
using MediatR;

namespace Banking.Application.Features.Accounts.Queries.GetAccountById;

public record GetAccountByIdQuery(Guid AccountId) : IRequest<AccountDto>;

public class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, AccountDto>
{
    private readonly IAccountRepository _accountRepository;

    public GetAccountByIdQueryHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<AccountDto> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account is null)
        {
            throw new EntityNotFoundException(nameof(Account), request.AccountId);
        }

        return new AccountDto(
            account.Id,
            account.CustomerId,
            account.Iban,
            account.Balance,
            account.Currency,
            account.IsActive,
            account.CreatedAt
        );
    }
}
