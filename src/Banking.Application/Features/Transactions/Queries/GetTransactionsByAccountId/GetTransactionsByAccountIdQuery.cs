using Banking.Application.Common.DTOs;
using Banking.Application.Common.Interfaces;
using Banking.Domain.Entities;
using Banking.Domain.Exceptions;
using MediatR;

namespace Banking.Application.Features.Transactions.Queries.GetTransactionsByAccountId;

public record GetTransactionsByAccountIdQuery(Guid AccountId) : IRequest<IReadOnlyList<TransactionDto>>;

public class GetTransactionsByAccountIdQueryHandler : IRequestHandler<GetTransactionsByAccountIdQuery, IReadOnlyList<TransactionDto>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;

    public GetTransactionsByAccountIdQueryHandler(
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<IReadOnlyList<TransactionDto>> Handle(GetTransactionsByAccountIdQuery request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account is null)
        {
            throw new EntityNotFoundException(nameof(Account), request.AccountId);
        }

        var transactions = await _transactionRepository.GetByAccountIdAsync(request.AccountId, cancellationToken);

        return transactions.Select(t => new TransactionDto(
            t.Id,
            t.SourceAccountId,
            t.TargetAccountId,
            t.Amount,
            t.Currency,
            t.TransactionType,
            t.TransactionDate,
            t.Description
        )).ToList();
    }
}
