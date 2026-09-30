using Banking.Application.Common.DTOs;
using Banking.Application.Common.Interfaces;
using Banking.Domain.Entities;
using Banking.Domain.Exceptions;
using MediatR;

namespace Banking.Application.Features.Accounts.Queries.GetAccountBalance;

public record GetAccountBalanceQuery(Guid AccountId) : IRequest<AccountBalanceDto>;

public class GetAccountBalanceQueryHandler : IRequestHandler<GetAccountBalanceQuery, AccountBalanceDto>
{
    private readonly IAccountRepository _accountRepository;

    public GetAccountBalanceQueryHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<AccountBalanceDto> Handle(GetAccountBalanceQuery request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account is null)
        {
            throw new EntityNotFoundException(nameof(Account), request.AccountId);
        }

        return new AccountBalanceDto(
            account.Id,
            account.Iban,
            account.Balance,
            account.Currency,
            account.IsActive
        );
    }
}
