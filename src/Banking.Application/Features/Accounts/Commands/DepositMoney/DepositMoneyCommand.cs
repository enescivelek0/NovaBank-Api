using Banking.Application.Common.DTOs;
using Banking.Application.Common.Interfaces;
using Banking.Domain.Entities;
using Banking.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace Banking.Application.Features.Accounts.Commands.DepositMoney;

public record DepositMoneyCommand(
    Guid AccountId,
    decimal Amount,
    string? Description = null
) : IRequest<AccountBalanceDto>;

public class DepositMoneyCommandValidator : AbstractValidator<DepositMoneyCommand>
{
    public DepositMoneyCommandValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty().WithMessage("Hesap ID alanı zorunludur.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Yatırılacak tutar sıfırdan büyük olmalıdır.");
    }
}

public class DepositMoneyCommandHandler : IRequestHandler<DepositMoneyCommand, AccountBalanceDto>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DepositMoneyCommandHandler(
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository,
        IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AccountBalanceDto> Handle(DepositMoneyCommand request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account is null)
        {
            throw new EntityNotFoundException(nameof(Account), request.AccountId);
        }

        account.Deposit(request.Amount);

        var transaction = Transaction.CreateDeposit(
            account.Id,
            request.Amount,
            account.Currency,
            request.Description
        );

        await _transactionRepository.AddAsync(transaction, cancellationToken);
        _accountRepository.Update(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AccountBalanceDto(
            account.Id,
            account.Iban,
            account.Balance,
            account.Currency,
            account.IsActive
        );
    }
}
