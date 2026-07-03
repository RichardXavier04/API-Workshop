using CardTransactions.Business.Interfaces;
using CardTransactions.Contracts.Requests;
using CardTransactions.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;

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
        var response = await _cardService.CreateCardAsync(request, cancellationToken);

        return Created($"/api/v1/cards/{response.Id}", response);
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
}