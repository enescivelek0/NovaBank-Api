using Banking.Application.Common.DTOs;
using Banking.Application.Common.Interfaces;
using MediatR;

namespace Banking.Application.Features.Accounts.Queries.GetAccountsByCustomerId;

public record GetAccountsByCustomerIdQuery(Guid CustomerId) : IRequest<IReadOnlyList<AccountDto>>;

public class GetAccountsByCustomerIdQueryHandler : IRequestHandler<GetAccountsByCustomerIdQuery, IReadOnlyList<AccountDto>>
{
    private readonly IAccountRepository _accountRepository;

    public GetAccountsByCustomerIdQueryHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<IReadOnlyList<AccountDto>> Handle(GetAccountsByCustomerIdQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _accountRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);

        return accounts.Select(a => new AccountDto(
            a.Id,
            a.CustomerId,
            a.Iban,
            a.Balance,
            a.Currency,
            a.IsActive,
            a.CreatedAt
        )).ToList();
    }
}
