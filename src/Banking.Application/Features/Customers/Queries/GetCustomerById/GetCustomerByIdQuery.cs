using Banking.Application.Common.DTOs;
using Banking.Application.Common.Interfaces;
using Banking.Domain.Entities;
using Banking.Domain.Exceptions;
using MediatR;

namespace Banking.Application.Features.Customers.Queries.GetCustomerById;

public record GetCustomerByIdQuery(Guid CustomerId) : IRequest<CustomerDto>;

public class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto>
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomerByIdQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<CustomerDto> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer is null)
        {
            throw new EntityNotFoundException(nameof(Customer), request.CustomerId);
        }

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
