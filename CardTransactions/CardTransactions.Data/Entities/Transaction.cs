namespace CardTransactions.Data.Entities;

public class Transaction
{
    public Guid Id { get; set; }

    public Guid CardId { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateOnly TransactionDate { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public Card Card { get; set; } = null!;
}