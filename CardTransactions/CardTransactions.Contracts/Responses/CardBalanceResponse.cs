namespace CardTransactions.Contracts.Responses;

public class CardBalanceResponse
{
    public Guid CardId { get; set; }

    public decimal CreditLimit { get; set; }

    public decimal TotalTransactionAmount { get; set; }

    public decimal OriginalAvailableBalance { get; set; }

    public string OriginalCurrency { get; set; } = string.Empty;

    public string TargetCurrency { get; set; } = string.Empty;

    public decimal ExchangeRateUsed { get; set; }

    public decimal ConvertedAvailableBalance { get; set; }
}