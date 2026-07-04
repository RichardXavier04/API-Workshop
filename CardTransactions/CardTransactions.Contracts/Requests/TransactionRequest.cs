namespace CardTransactions.Contracts.Requests;

public class TransactionRequest
{
    public string Description { get; set; } = string.Empty;

    public DateOnly TransactionDate { get; set; }

    public decimal Amount { get; set; }
}