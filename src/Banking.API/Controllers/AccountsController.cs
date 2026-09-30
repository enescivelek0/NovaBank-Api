using Banking.Application.Common.DTOs;
using Banking.Application.Features.Accounts.Commands.CreateAccount;
using Banking.Application.Features.Accounts.Commands.DepositMoney;
using Banking.Application.Features.Accounts.Queries.GetAccountBalance;
using Banking.Application.Features.Accounts.Queries.GetAccountById;
using Banking.Application.Features.Transactions.Queries.GetTransactionsByAccountId;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Banking.API.Controllers;

[Authorize]
public class AccountsController : ApiControllerBase
{
    /// <summary>
    /// Müşteri için yeni bir banka hesabı oluşturur (TR formatında otomatik IBAN üretilir).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AccountDto>> Create([FromBody] CreateAccountCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Hesap detay bilgilerini getirir.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountDto>> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetAccountByIdQuery(id));
        return Ok(result);
    }

    /// <summary>
    /// Hesabın güncel bakiye bilgisini döner.
    /// </summary>
    [HttpGet("{id:guid}/balance")]
    [ProducesResponseType(typeof(AccountBalanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountBalanceDto>> GetBalance(Guid id)
    {
        var result = await Mediator.Send(new GetAccountBalanceQuery(id));
        return Ok(result);
    }

    /// <summary>
    /// Hesaba para yatırma işlemi gerçekleştirir.
    /// </summary>
    [HttpPost("{id:guid}/deposit")]
    [ProducesResponseType(typeof(AccountBalanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountBalanceDto>> Deposit(Guid id, [FromBody] DepositMoneyRequest request)
    {
        var command = new DepositMoneyCommand(id, request.Amount, request.Description);
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Hesaba ait tüm hesap hareketlerini (Transfer, Yatırma, Çekme) listeler.
    /// </summary>
    [HttpGet("{id:guid}/transactions")]
    [ProducesResponseType(typeof(IReadOnlyList<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<TransactionDto>>> GetTransactions(Guid id)
    {
        var result = await Mediator.Send(new GetTransactionsByAccountIdQuery(id));
        return Ok(result);
    }
}

public record DepositMoneyRequest(decimal Amount, string? Description = null);
