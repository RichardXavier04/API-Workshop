namespace CardTransactions.Business.Interfaces;

public interface ICurrencyConversionService
{
    Task<(decimal ExchangeRateUsed, decimal ConvertedAmount)> ConvertAsync(
        decimal amount,
        string sourceCurrency,
        string targetCurrency,
        DateOnly transactionDate,
        CancellationToken cancellationToken = default);

    Task<(decimal ExchangeRateUsed, decimal ConvertedAmount)> ConvertWithLatestRateAsync(
     decimal amount,
     string sourceCurrency,
     string targetCurrency,
     CancellationToken cancellationToken = default);
}