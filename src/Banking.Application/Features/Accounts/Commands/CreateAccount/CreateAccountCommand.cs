using Banking.Application.Common.DTOs;
using Banking.Application.Common.Interfaces;
using Banking.Domain.Entities;
using Banking.Domain.Enums;
using Banking.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace Banking.Application.Features.Accounts.Commands.CreateAccount;

public record CreateAccountCommand(
    Guid CustomerId,
    Currency Currency,
    decimal InitialBalance = 0m
) : IRequest<AccountDto>;

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Müşteri ID alanı zorunludur.");

        RuleFor(x => x.Currency)
            .IsInEnum().WithMessage("Geçersiz para birimi seçimi.");

        RuleFor(x => x.InitialBalance)
            .GreaterThanOrEqualTo(0).WithMessage("Başlangıç bakiyesi negatif olamaz.");
    }
}

public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, AccountDto>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAccountCommandHandler(
        IAccountRepository accountRepository,
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AccountDto> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer is null)
        {
            throw new EntityNotFoundException(nameof(Customer), request.CustomerId);
        }

        var account = new Account(request.CustomerId, request.Currency, request.InitialBalance);

        await _accountRepository.AddAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
