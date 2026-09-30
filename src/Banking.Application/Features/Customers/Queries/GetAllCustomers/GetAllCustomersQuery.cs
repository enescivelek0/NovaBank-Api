using Banking.Application.Common.DTOs;
using Banking.Application.Common.Interfaces;
using MediatR;

namespace Banking.Application.Features.Customers.Queries.GetAllCustomers;

public record GetAllCustomersQuery : IRequest<IReadOnlyList<CustomerDto>>;

public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, IReadOnlyList<CustomerDto>>
{
    private readonly ICustomerRepository _customerRepository;

    public GetAllCustomersQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<IReadOnlyList<CustomerDto>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = await _customerRepository.ListAllAsync(cancellationToken);

        return customers.Select(c => new CustomerDto(
            c.Id,
            c.FirstName,
            c.LastName,
            c.FullName,
            c.Email,
            c.IdentityNumber,
            c.CreatedAt
        )).ToList();
    }
}
