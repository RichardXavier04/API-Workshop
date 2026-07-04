namespace CardTransactions.Contracts.Responses;

public class CardResponse
{
    public Guid Id { get; set; }

    public decimal CreditLimit { get; set; }

    public string Currency { get; set; } = "AUD";

    public DateTime CreatedAt { get; set; }
}