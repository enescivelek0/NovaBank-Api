using Banking.Application.Common.DTOs;
using Banking.Application.Common.Interfaces;
using Banking.Domain.Entities;
using Banking.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace Banking.Application.Features.Customers.Commands.CreateCustomer;

public record CreateCustomerCommand(
    string FirstName,
    string LastName,
    string Email,
    string IdentityNumber
) : IRequest<CustomerDto>;

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ad alanı boş bırakılamaz.")
            .MaximumLength(50).WithMessage("Ad en fazla 50 karakter olabilir.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Soyad alanı boş bırakılamaz.")
            .MaximumLength(50).WithMessage("Soyad en fazla 50 karakter olabilir.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi boş bırakılamaz.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi girilmelidir.");

        RuleFor(x => x.IdentityNumber)
            .NotEmpty().WithMessage("Kimlik numarası boş bırakılamaz.")
            .Matches(@"^\d{10,11}$").WithMessage("Kimlik numarası 10 veya 11 rakamdan oluşmalıdır.");
    }
}

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomerCommandHandler(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var existingEmail = await _customerRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingEmail is not null)
        {
            throw new InvalidAccountOperationException($"'{request.Email}' e-posta adresiyle kayıtlı bir müşteri zaten mevcut.");
        }

        var existingIdentity = await _customerRepository.GetByIdentityNumberAsync(request.IdentityNumber, cancellationToken);
        if (existingIdentity is not null)
        {
            throw new InvalidAccountOperationException($"'{request.IdentityNumber}' kimlik numarasıyla kayıtlı bir müşteri zaten mevcut.");
        }

        var customer = new Customer(request.FirstName, request.LastName, request.Email, request.IdentityNumber);

        await _customerRepository.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CustomerDto(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.FullName,
            customer.Email,
            customer.IdentityNumber,
            customer.CreatedAt
        );
    }
}
