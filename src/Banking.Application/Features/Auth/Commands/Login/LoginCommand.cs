using Banking.Application.Common.DTOs;
using Banking.Application.Common.Interfaces;
using Banking.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace Banking.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string IdentityNumber) : IRequest<AuthResponseDto>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi boş bırakılamaz.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");

        RuleFor(x => x.IdentityNumber)
            .NotEmpty().WithMessage("Kimlik numarası boş bırakılamaz.");
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(ICustomerRepository customerRepository, ITokenService tokenService)
    {
        _customerRepository = customerRepository;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (customer is null || customer.IdentityNumber != request.IdentityNumber)
        {
            throw new InvalidAccountOperationException("Geçersiz e-posta veya kimlik numarası.");
        }

        var token = _tokenService.GenerateToken(customer);
        var expiresAt = DateTime.UtcNow.AddHours(4);

        return new AuthResponseDto(
            token,
            customer.Id,
            customer.FullName,
            customer.Email,
            expiresAt
        );
    }
}
