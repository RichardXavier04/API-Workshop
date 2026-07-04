using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace CardTransactions.Data.Clients;

public class TreasuryExchangeRateClient : ITreasuryExchangeRateClient
{
    private const string TreasuryApiBaseUrl =
        "https://api.fiscaldata.treasury.gov/services/api/fiscal_service/v1/accounting/od/rates_of_exchange";

    private static readonly Dictionary<string, string> IsoToTreasuryCountry =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["AUD"] = "Australia",
            ["EUR"] = "Euro Zone",
            ["GBP"] = "United Kingdom",
            ["NZD"] = "New Zealand",
            ["CAD"] = "Canada",
            ["JPY"] = "Japan"
        };

    private readonly HttpClient _httpClient;

    public TreasuryExchangeRateClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<TreasuryExchangeRate>> GetRatesAsync(
        string currency,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        if (!IsoToTreasuryCountry.TryGetValue(currency, out var treasuryCountry))
        {
            return Array.Empty<TreasuryExchangeRate>();
        }

        var url =
            $"{TreasuryApiBaseUrl}" +
            "?fields=record_date,country,currency,exchange_rate" +
            $"&filter=country:eq:{Uri.EscapeDataString(treasuryCountry)},record_date:gte:{fromDate:yyyy-MM-dd},record_date:lte:{toDate:yyyy-MM-dd}" +
            "&sort=-record_date" +
            "&page[size]=10000";

        var response = await _httpClient.GetFromJsonAsync<TreasuryApiResponse>(
            url,
            cancellationToken);

        if (response?.Data is null || response.Data.Count == 0)
        {
            return Array.Empty<TreasuryExchangeRate>();
        }

        return response.Data
            .Select(x => new TreasuryExchangeRate
            {
                RecordDate = DateOnly.Parse(x.RecordDate, CultureInfo.InvariantCulture),
                Currency = currency.ToUpperInvariant(),
                ExchangeRate = decimal.Parse(x.ExchangeRate, CultureInfo.InvariantCulture)
            })
            .ToList();
    }

    public async Task<TreasuryExchangeRate?> GetLatestRateAsync(string currency, CancellationToken cancellationToken = default)
    {
        if (!IsoToTreasuryCountry.TryGetValue(currency, out var treasuryCountry))
        {
            return null;
        }

        var url =
            $"{TreasuryApiBaseUrl}" +
            "?fields=record_date,country,currency,exchange_rate" +
            $"&filter=country:eq:{Uri.EscapeDataString(treasuryCountry)}" +
            "&sort=-record_date" +
            "&page[size]=1";

        var response = await _httpClient.GetFromJsonAsync<TreasuryApiResponse>(url, cancellationToken);

        var latest = response?.Data?.FirstOrDefault();
        if (latest is null)
        {
            return null;
        }

        return new TreasuryExchangeRate
        {
            RecordDate = DateOnly.Parse(latest.RecordDate, CultureInfo.InvariantCulture),
            Currency = currency.ToUpperInvariant(),
            ExchangeRate = decimal.Parse(latest.ExchangeRate, CultureInfo.InvariantCulture)
        };
    }

    private sealed class TreasuryApiResponse
    {
        [JsonPropertyName("data")]
        public List<TreasuryApiRate> Data { get; set; } = [];
    }

    private sealed class TreasuryApiRate
    {
        [JsonPropertyName("record_date")]
        public string RecordDate { get; set; } = string.Empty;

        [JsonPropertyName("exchange_rate")]
        public string ExchangeRate { get; set; } = string.Empty;
    }

}