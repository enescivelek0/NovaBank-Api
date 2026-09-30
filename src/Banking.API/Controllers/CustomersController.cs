using Banking.Application.Common.DTOs;
using Banking.Application.Features.Customers.Commands.CreateCustomer;
using Banking.Application.Features.Customers.Queries.GetAllCustomers;
using Banking.Application.Features.Customers.Queries.GetCustomerById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Banking.API.Controllers;

[Authorize]
public class CustomersController : ApiControllerBase
{
    /// <summary>
    /// Sistemde kayıtlı tüm müşterileri listeler.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CustomerDto>>> GetAll()
    {
        var result = await Mediator.Send(new GetAllCustomersQuery());
        return Ok(result);
    }

    /// <summary>
    /// Belirtilen ID'ye sahip müşteriyi getirir.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetCustomerByIdQuery(id));
        return Ok(result);
    }

    /// <summary>
    /// Yeni müşteri kaydı oluşturur.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CustomerDto>> Create([FromBody] CreateCustomerCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
