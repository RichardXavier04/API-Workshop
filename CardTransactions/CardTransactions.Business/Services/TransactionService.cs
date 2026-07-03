using CardTransactions.Business.Interfaces;
using CardTransactions.Contracts.Requests;
using CardTransactions.Contracts.Responses;
using CardTransactions.Data.Entities;
using CardTransactions.Data.Repositories;

namespace CardTransactions.Business.Services;

public class TransactionService : ITransactionService
{
    private const string DefaultCurrency = "AUD";

    private readonly ICardRepository _cardRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICurrencyConversionService _currencyConversionService;

    public TransactionService(
        ICardRepository cardRepository,
        ITransactionRepository transactionRepository,
        ICurrencyConversionService currencyConversionService)
    {
        _cardRepository = cardRepository;
        _transactionRepository = transactionRepository;
        _currencyConversionService = currencyConversionService;
    }

    public async Task<TransactionResponse> CreateTransactionAsync(
        Guid cardId,
        TransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Description))
        {
            throw new ArgumentException("Description is required.", nameof(request));
        }

        if (request.Amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than 0.", nameof(request));
        }

        if (request.TransactionDate == default)
        {
            throw new ArgumentException("TransactionDate is required.", nameof(request));
        }

        var cardExists = await _cardRepository.ExistsAsync(cardId, cancellationToken);
        if (!cardExists)
        {
            throw new KeyNotFoundException($"Card '{cardId}' was not found.");
        }

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            CardId = cardId,
            Description = request.Description.Trim(),
            TransactionDate = request.TransactionDate,
            Amount = request.Amount,
            Currency = DefaultCurrency,
            CreatedAt = DateTime.UtcNow
        };

        await _transactionRepository.AddAsync(transaction, cancellationToken);
        await _transactionRepository.SaveChangesAsync(cancellationToken);

        return new TransactionResponse
        {
            Id = transaction.Id,
            CardId = transaction.CardId,
            Description = transaction.Description,
            TransactionDate = transaction.TransactionDate,
            Amount = transaction.Amount,
            Currency = transaction.Currency
        };
    }

    public async Task<ConvertedTransactionResponse> GetTransactionInCurrencyAsync(
    Guid transactionId,
    string currency,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency is required.", nameof(currency));
        }

        var transaction = await _transactionRepository.GetByIdAsync(transactionId, cancellationToken);
        if (transaction is null)
        {
            throw new KeyNotFoundException($"Transaction '{transactionId}' was not found.");
        }

        var targetCurrency = currency.Trim().ToUpperInvariant();

        var (exchangeRateUsed, convertedAmount) = await _currencyConversionService.ConvertAsync(
            transaction.Amount,
            transaction.Currency,
            targetCurrency,
            transaction.TransactionDate,
            cancellationToken);

        return new ConvertedTransactionResponse
        {
            Id = transaction.Id,
            Description = transaction.Description,
            TransactionDate = transaction.TransactionDate,
            OriginalAmount = transaction.Amount,
            OriginalCurrency = transaction.Currency,
            TargetCurrency = targetCurrency,
            ExchangeRateUsed = exchangeRateUsed,
            ConvertedAmount = convertedAmount
        };
    }
}
