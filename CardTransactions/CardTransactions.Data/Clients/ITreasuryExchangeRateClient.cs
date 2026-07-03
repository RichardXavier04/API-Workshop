namespace CardTransactions.Data.Clients;

public interface ITreasuryExchangeRateClient
{
    Task<IReadOnlyList<TreasuryExchangeRate>> GetRatesAsync(
        string currency,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default);
}