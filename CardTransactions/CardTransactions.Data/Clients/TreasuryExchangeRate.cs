namespace CardTransactions.Data.Clients;

public class TreasuryExchangeRate
{
    public DateOnly RecordDate { get; set; }

    public string Currency { get; set; } = string.Empty;

    public decimal ExchangeRate { get; set; }
}