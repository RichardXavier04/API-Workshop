using CardTransactions.Business.Interfaces;
using CardTransactions.Contracts.Requests;
using CardTransactions.Contracts.Responses;
using CardTransactions.Data.Entities;
using CardTransactions.Data.Repositories;

namespace CardTransactions.Business.Services;

public class CardService : ICardService
{
    private const string DefaultCurrency = "AUD";

    private readonly ICardRepository _cardRepository;

    public CardService(ICardRepository cardRepository)
    {
        _cardRepository = cardRepository;
    }

    public async Task<CardResponse> CreateCardAsync(
        CreateCardRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.CreditLimit <= 0)
        {
            throw new ArgumentException("CreditLimit must be greater than 0.", nameof(request));
        }

        var card = new Card
        {
            Id = Guid.NewGuid(),
            CreditLimit = request.CreditLimit,
            Currency = DefaultCurrency,
            CreatedAt = DateTime.UtcNow
        };

        await _cardRepository.AddAsync(card, cancellationToken);
        await _cardRepository.SaveChangesAsync(cancellationToken);

        return new CardResponse
        {
            Id = card.Id,
            CreditLimit = card.CreditLimit,
            Currency = card.Currency,
            CreatedAt = card.CreatedAt
        };
    }
}