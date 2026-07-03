using CardTransactions.Business.Exceptions;
using CardTransactions.Business.Interfaces;
using CardTransactions.Data.Clients;

namespace CardTransactions.Business.Services;

public class CurrencyConversionService : ICurrencyConversionService
{
    private const string UsdCurrency = "USD";

    private readonly ITreasuryExchangeRateClient _treasuryClient;

    public CurrencyConversionService(ITreasuryExchangeRateClient treasuryClient)
    {
        _treasuryClient = treasuryClient;
    }

    public async Task<(decimal ExchangeRateUsed, decimal ConvertedAmount)> ConvertAsync(
        decimal amount,
        string sourceCurrency,
        string targetCurrency,
        DateOnly transactionDate,
        CancellationToken cancellationToken = default)
    {
        sourceCurrency = sourceCurrency.ToUpperInvariant();
        targetCurrency = targetCurrency.ToUpperInvariant();

        if (sourceCurrency == targetCurrency)
        {
            return (1m, amount);
        }

        var windowStart = transactionDate.AddMonths(-6);
        var amountInUsd = await ConvertToUsdAsync(amount, sourceCurrency, transactionDate, windowStart, cancellationToken);
        var convertedAmount = await ConvertFromUsdAsync(amountInUsd, targetCurrency, transactionDate, windowStart, cancellationToken);

        var effectiveRate = amount == 0 ? 0 : decimal.Round(convertedAmount / amount, 6);

        return (effectiveRate, decimal.Round(convertedAmount, 2));
    }

    private async Task<decimal> ConvertToUsdAsync(
        decimal amount,
        string sourceCurrency,
        DateOnly transactionDate,
        DateOnly windowStart,
        CancellationToken cancellationToken)
    {
        if (sourceCurrency == UsdCurrency)
        {
            return amount;
        }

        var rate = await GetHistoricalRateAsync(sourceCurrency, transactionDate, windowStart, cancellationToken);
        return amount / rate;
    }

    private async Task<decimal> ConvertFromUsdAsync(
        decimal amountInUsd,
        string targetCurrency,
        DateOnly transactionDate,
        DateOnly windowStart,
        CancellationToken cancellationToken)
    {
        if (targetCurrency == UsdCurrency)
        {
            return amountInUsd;
        }

        var rate = await GetHistoricalRateAsync(targetCurrency, transactionDate, windowStart, cancellationToken);
        return amountInUsd * rate;
    }

    private async Task<decimal> GetHistoricalRateAsync(
        string currency,
        DateOnly transactionDate,
        DateOnly windowStart,
        CancellationToken cancellationToken)
    {
        var rates = await _treasuryClient.GetRatesAsync(
            currency, windowStart, transactionDate, cancellationToken);

        var selectedRate = rates
            .Where(x => x.RecordDate <= transactionDate)
            .OrderByDescending(x => x.RecordDate)
            .FirstOrDefault();

        if (selectedRate is null)
        {
            throw new CurrencyConversionException(
                "The transaction cannot be converted to the target currency.");
        }

        return selectedRate.ExchangeRate;
    }
}