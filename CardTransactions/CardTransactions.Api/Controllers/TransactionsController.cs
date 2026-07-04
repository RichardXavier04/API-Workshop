using CardTransactions.Business.Exceptions;
using CardTransactions.Business.Interfaces;
using CardTransactions.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;

namespace CardTransactions.Api.Controllers;

[ApiController]
[Route("api/v1/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpGet("{transactionId:guid}")]
    [ProducesResponseType(typeof(ConvertedTransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConvertedTransactionResponse>> Get(
        [FromRoute] Guid transactionId,
        [FromQuery] string currency,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _transactionService.GetTransactionInCurrencyAsync(
                transactionId, currency, cancellationToken);

            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (CurrencyConversionException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}