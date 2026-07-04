using CardTransactions.Business.Interfaces;
using CardTransactions.Contracts.Requests;
using CardTransactions.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;
using CardTransactions.Business.Exceptions;

namespace CardTransactions.Api.Controllers;

[ApiController]
[Route("api/v1/cards")]
public class CardsController : ControllerBase
{
    private readonly ICardService _cardService;
    private readonly ITransactionService _transactionService;

    public CardsController(ICardService cardService, ITransactionService transactionService)
    {
        _cardService = cardService;
        _transactionService = transactionService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CardResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CardResponse>> Create(
    [FromBody] CreateCardRequest request,
    CancellationToken cancellationToken)
    {
        try
        {
            var response = await _cardService.CreateCardAsync(request, cancellationToken);

            return Created($"/api/v1/cards/{response.Id}", response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{cardId:guid}/transactions")]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionResponse>> CreateTransaction(
        [FromRoute] Guid cardId,
        [FromBody] TransactionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _transactionService.CreateTransactionAsync(
                cardId, request, cancellationToken);
            return Created(
                $"/api/v1/cards/{cardId}/transactions/{response.Id}",
                response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{cardId:guid}/balance")]
    [ProducesResponseType(typeof(CardBalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CardBalanceResponse>> GetBalance(
    [FromRoute] Guid cardId,
    [FromQuery] string currency,
    CancellationToken cancellationToken)
    {
        try
        {
            var response = await _cardService.GetBalanceInCurrencyAsync(
                cardId, currency, cancellationToken);

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