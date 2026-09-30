using Banking.Application.Common.DTOs;
using Banking.Application.Common.Interfaces;
using Banking.Domain.Entities;
using Banking.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace Banking.Application.Features.Accounts.Commands.TransferMoney;

public record TransferMoneyCommand(
    Guid SourceAccountId,
    Guid TargetAccountId,
    decimal Amount,
    string? Description = null
) : IRequest<TransactionDto>;

public class TransferMoneyCommandValidator : AbstractValidator<TransferMoneyCommand>
{
    public TransferMoneyCommandValidator()
    {
        RuleFor(x => x.SourceAccountId)
            .NotEmpty().WithMessage("Kaynak hesap ID alanı zorunludur.");

        RuleFor(x => x.TargetAccountId)
            .NotEmpty().WithMessage("Hedef hesap ID alanı zorunludur.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Transfer tutarı sıfırdan büyük olmalıdır.");

        RuleFor(x => x)
            .Must(x => x.SourceAccountId != x.TargetAccountId)
            .WithMessage("Kaynak ve hedef hesaplar aynı olamaz.");
    }
}

public class TransferMoneyCommandHandler : IRequestHandler<TransferMoneyCommand, TransactionDto>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TransferMoneyCommandHandler(
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository,
        IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TransactionDto> Handle(TransferMoneyCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var sourceAccount = await _accountRepository.GetByIdAsync(request.SourceAccountId, cancellationToken);
            if (sourceAccount is null)
            {
                throw new EntityNotFoundException(nameof(Account), request.SourceAccountId);
            }

            var targetAccount = await _accountRepository.GetByIdAsync(request.TargetAccountId, cancellationToken);
            if (targetAccount is null)
            {
                throw new EntityNotFoundException(nameof(Account), request.TargetAccountId);
            }

            // TransferTo handles business invariant:
            // 1. Checks if source has enough balance (throws InsufficientBalanceException if not)
            // 2. Checks active status of accounts
            // 3. Checks currency match
            // 4. Withdraws from source and deposits to target
            sourceAccount.TransferTo(targetAccount, request.Amount);

            var transaction = Transaction.CreateTransfer(
                sourceAccount.Id,
                targetAccount.Id,
                request.Amount,
                sourceAccount.Currency,
                request.Description
            );

            await _transactionRepository.AddAsync(transaction, cancellationToken);
            _accountRepository.Update(sourceAccount);
            _accountRepository.Update(targetAccount);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return new TransactionDto(
                transaction.Id,
                transaction.SourceAccountId,
                transaction.TargetAccountId,
                transaction.Amount,
                transaction.Currency,
                transaction.TransactionType,
                transaction.TransactionDate,
                transaction.Description
            );
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
