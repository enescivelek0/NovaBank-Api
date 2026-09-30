using Banking.Application.Common.DTOs;
using Banking.Application.Features.Auth.Commands.Login;
using Banking.Application.Features.Customers.Commands.CreateCustomer;
using Microsoft.AspNetCore.Mvc;

namespace Banking.API.Controllers;

public class AuthController : ApiControllerBase
{
    /// <summary>
    /// Müşteri e-posta ve kimlik numarası ile giriş yaparak JWT token üretir.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Yeni bir müşteri kaydı oluşturur ve otomatik giriş token'ı üretir.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] CreateCustomerCommand command)
    {
        var customer = await Mediator.Send(command);
        var auth = await Mediator.Send(new LoginCommand(customer.Email, customer.IdentityNumber));
        return CreatedAtAction(nameof(Login), auth);
    }
}
