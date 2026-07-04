using CardTransactions.Data.Clients;

namespace CardTransactions.IntegrationTests.Support;

public sealed class FakeTreasuryExchangeRateClient : ITreasuryExchangeRateClient
{
    private static readonly DateOnly HistoricalRateDate = new(2024, 12, 31);
    private static readonly DateOnly LatestRateDate = new(2026, 3, 31);

    public bool ReturnEmptyHistoricalRates { get; set; }

    public Task<IReadOnlyList<TreasuryExchangeRate>> GetRatesAsync(
        string currency,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        if (ReturnEmptyHistoricalRates)
        {
            return Task.FromResult<IReadOnlyList<TreasuryExchangeRate>>(Array.Empty<TreasuryExchangeRate>());
        }

        if (!TryGetHistoricalRate(currency, out var rate))
        {
            return Task.FromResult<IReadOnlyList<TreasuryExchangeRate>>(Array.Empty<TreasuryExchangeRate>());
        }

        if (HistoricalRateDate < fromDate || HistoricalRateDate > toDate)
        {
            return Task.FromResult<IReadOnlyList<TreasuryExchangeRate>>(Array.Empty<TreasuryExchangeRate>());
        }

        IReadOnlyList<TreasuryExchangeRate> rates =
        [
            new TreasuryExchangeRate
            {
                RecordDate = HistoricalRateDate,
                Currency = currency.ToUpperInvariant(),
                ExchangeRate = rate
            }
        ];

        return Task.FromResult(rates);
    }

    public Task<TreasuryExchangeRate?> GetLatestRateAsync(
        string currency,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetLatestRate(currency, out var rate))
        {
            return Task.FromResult<TreasuryExchangeRate?>(null);
        }

        return Task.FromResult<TreasuryExchangeRate?>(new TreasuryExchangeRate
        {
            RecordDate = LatestRateDate,
            Currency = currency.ToUpperInvariant(),
            ExchangeRate = rate
        });
    }

    private static bool TryGetHistoricalRate(string currency, out decimal rate) =>
        currency.ToUpperInvariant() switch
        {
            "AUD" => SetRate(out rate, 1.612m),
            "EUR" => SetRate(out rate, 0.962m),
            _ => FailRate(out rate)
        };

    private static bool TryGetLatestRate(string currency, out decimal rate) =>
        currency.ToUpperInvariant() switch
        {
            "AUD" => SetRate(out rate, 1.453m),
            "EUR" => SetRate(out rate, 0.851m),
            _ => FailRate(out rate)
        };

    private static bool SetRate(out decimal rate, decimal value)
    {
        rate = value;
        return true;
    }

    private static bool FailRate(out decimal rate)
    {
        rate = 0m;
        return false;
    }
}