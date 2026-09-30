using Banking.Application.Common.DTOs;
using Banking.Application.Features.Accounts.Commands.TransferMoney;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Banking.API.Controllers;

[Authorize]
public class TransactionsController : ApiControllerBase
{
    /// <summary>
    /// İki hesap arasında atomik (Database Transaction & Unit of Work) para transferi gerçekleştirir.
    /// </summary>
    [HttpPost("transfer")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionDto>> Transfer([FromBody] TransferMoneyCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }
}
