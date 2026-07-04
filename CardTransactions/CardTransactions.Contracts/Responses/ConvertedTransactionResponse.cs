namespace CardTransactions.Contracts.Responses;

public class ConvertedTransactionResponse
{
    public Guid Id { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateOnly TransactionDate { get; set; }

    public decimal OriginalAmount { get; set; }

    public string OriginalCurrency { get; set; } = string.Empty;

    public string TargetCurrency { get; set; } = string.Empty;

    public decimal ExchangeRateUsed { get; set; }

    public decimal ConvertedAmount { get; set; }
}