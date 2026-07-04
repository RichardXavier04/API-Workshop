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

    private readonly ITransactionRepository _transactionRepository;

    private readonly ICurrencyConversionService _currencyConversionService;

    public CardService(ICardRepository cardRepository,
        ITransactionRepository transactionRepository,
        ICurrencyConversionService currencyConversionService)
    {
        _cardRepository = cardRepository;
        _transactionRepository = transactionRepository;
        _currencyConversionService = currencyConversionService;
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
    public async Task<CardBalanceResponse> GetBalanceInCurrencyAsync(
    Guid cardId,
    string currency,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency is required.", nameof(currency));
        }

        var card = await _cardRepository.GetByIdAsync(cardId, cancellationToken);
        if (card is null)
        {
            throw new KeyNotFoundException($"Card '{cardId}' was not found.");
        }

        var totalSpent = await _transactionRepository.GetTotalAmountByCardIdAsync(
            cardId, cancellationToken);

        var availableBalance = card.CreditLimit - totalSpent;
        var targetCurrency = currency.Trim().ToUpperInvariant();

        var (exchangeRateUsed, convertedBalance) =
            await _currencyConversionService.ConvertWithLatestRateAsync(
                availableBalance,
                card.Currency,
                targetCurrency,
                cancellationToken);

        return new CardBalanceResponse
        {
            CardId = card.Id,
            CreditLimit = card.CreditLimit,
            TotalTransactionAmount = totalSpent,
            OriginalAvailableBalance = availableBalance,
            OriginalCurrency = card.Currency,
            TargetCurrency = targetCurrency,
            ExchangeRateUsed = exchangeRateUsed,
            ConvertedAvailableBalance = convertedBalance
        };
    }
}