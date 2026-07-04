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
        var windowStart = transactionDate.AddMonths(-6);

        return await ConvertCoreAsync(
            amount,
            sourceCurrency,
            targetCurrency,
            (currency, ct) => GetHistoricalRateAsync(currency, transactionDate, windowStart, ct),
            cancellationToken);
    }

    public async Task<(decimal ExchangeRateUsed, decimal ConvertedAmount)> ConvertWithLatestRateAsync(
        decimal amount,
        string sourceCurrency,
        string targetCurrency,
        CancellationToken cancellationToken = default)
    {
        return await ConvertCoreAsync(
            amount,
            sourceCurrency,
            targetCurrency,
            GetLatestRateOrThrowAsync,
            cancellationToken);
    }

    private async Task<(decimal ExchangeRateUsed, decimal ConvertedAmount)> ConvertCoreAsync(
        decimal amount,
        string sourceCurrency,
        string targetCurrency,
        Func<string, CancellationToken, Task<decimal>> getRateAsync,
        CancellationToken cancellationToken)
    {
        sourceCurrency = sourceCurrency.ToUpperInvariant();
        targetCurrency = targetCurrency.ToUpperInvariant();

        if (sourceCurrency == targetCurrency)
        {
            return (1m, amount);
        }

        var amountInUsd = await ConvertToUsdAsync(
            amount, sourceCurrency, getRateAsync, cancellationToken);

        var convertedAmount = await ConvertFromUsdAsync(
            amountInUsd, targetCurrency, getRateAsync, cancellationToken);

        var effectiveRate = amount == 0 ? 0 : decimal.Round(convertedAmount / amount, 6);

        return (effectiveRate, decimal.Round(convertedAmount, 2));
    }

    private static async Task<decimal> ConvertToUsdAsync(
        decimal amount,
        string sourceCurrency,
        Func<string, CancellationToken, Task<decimal>> getRateAsync,
        CancellationToken cancellationToken)
    {
        if (sourceCurrency == UsdCurrency)
        {
            return amount;
        }

        var rate = await getRateAsync(sourceCurrency, cancellationToken);
        return amount / rate;
    }

    private static async Task<decimal> ConvertFromUsdAsync(
        decimal amountInUsd,
        string targetCurrency,
        Func<string, CancellationToken, Task<decimal>> getRateAsync,
        CancellationToken cancellationToken)
    {
        if (targetCurrency == UsdCurrency)
        {
            return amountInUsd;
        }

        var rate = await getRateAsync(targetCurrency, cancellationToken);
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

    private async Task<decimal> GetLatestRateOrThrowAsync(
        string currency,
        CancellationToken cancellationToken)
    {
        var latestRate = await _treasuryClient.GetLatestRateAsync(currency, cancellationToken);

        if (latestRate is null)
        {
            throw new CurrencyConversionException(
                "The balance cannot be converted to the target currency.");
        }

        return latestRate.ExchangeRate;
    }
}