using System.Net;
using System.Net.Http.Json;
using CardTransactions.Contracts.Requests;
using CardTransactions.Contracts.Responses;
using CardTransactions.Data;
using CardTransactions.IntegrationTests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace CardTransactions.IntegrationTests;

public class CardTransactionsApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CardTransactionsApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.FakeTreasury.ReturnEmptyHistoricalRates = false;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateCard_WhenRequestIsValid_ReturnsCreatedAndPersistsCard()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/cards",
            new CreateCardRequest { CreditLimit = 5000m });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<CardResponse>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.Id);
        Assert.Equal(5000m, body.CreditLimit);
        Assert.Equal("AUD", body.Currency);

        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CardTransactionsDbContext>();
        Assert.True(await dbContext.Cards.FindAsync(body.Id) is not null);
    }

    [Fact]
    public async Task CreateTransaction_WhenCardExists_ReturnsCreatedAndPersistsTransaction()
    {
        var cardId = await CreateCardAsync(5000m);
        var transactionDate = YesterdayUtc();

        var response = await _client.PostAsJsonAsync(
            $"/api/v1/cards/{cardId}/transactions",
            new TransactionRequest
            {
                Description = "Marriott Hotel Sydney",
                Amount = 450m,
                TransactionDate = transactionDate
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<TransactionResponse>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.Id);
        Assert.Equal(cardId, body.CardId);
        Assert.Equal("Marriott Hotel Sydney", body.Description);
        Assert.Equal(450m, body.Amount);
        Assert.Equal(transactionDate, body.TransactionDate);
        Assert.Equal("AUD", body.Currency);

        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CardTransactionsDbContext>();
        Assert.True(await dbContext.Transactions.FindAsync(body.Id) is not null);
    }

    [Fact]
    public async Task GetTransactionInCurrency_WhenTreasuryRateExists_ReturnsConvertedTransaction()
    {
        _factory.FakeTreasury.ReturnEmptyHistoricalRates = false;

        var cardId = await CreateCardAsync(5000m);
        var transactionDate = new DateOnly(2025, 3, 15);

        var transactionId = await CreateTransactionAsync(
            cardId,
            750m,
            transactionDate,
            "Requirement 3 EUR Test");

        var response = await _client.GetAsync(
            $"/api/v1/transactions/{transactionId}?currency=EUR");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ConvertedTransactionResponse>();
        Assert.NotNull(body);
        Assert.Equal(transactionId, body.Id);
        Assert.Equal(750m, body.OriginalAmount);
        Assert.Equal("AUD", body.OriginalCurrency);
        Assert.Equal("EUR", body.TargetCurrency);
        Assert.Equal(447.58m, body.ConvertedAmount);
        Assert.Equal(0.596774m, body.ExchangeRateUsed);
    }

    [Fact]
    public async Task GetTransactionInCurrency_WhenNoHistoricalRateExists_ReturnsBadRequest()
    {
        _factory.FakeTreasury.ReturnEmptyHistoricalRates = true;

        var cardId = await CreateCardAsync(5000m);

        var transactionId = await CreateTransactionAsync(
            cardId,
            450m,
            YesterdayUtc(),
            "No rate test");

        var response = await _client.GetAsync(
            $"/api/v1/transactions/{transactionId}?currency=EUR");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var message = await response.Content.ReadAsStringAsync();
        Assert.Contains("cannot be converted", message, StringComparison.OrdinalIgnoreCase);

        _factory.FakeTreasury.ReturnEmptyHistoricalRates = false;
    }

    [Fact]
    public async Task GetBalance_WhenTransactionsExist_ReturnsConvertedAvailableBalance()
    {
        _factory.FakeTreasury.ReturnEmptyHistoricalRates = false;

        var cardId = await CreateCardAsync(5000m);
        await CreateTransactionAsync(cardId, 450m, YesterdayUtc(), "Transaction 1");
        await CreateTransactionAsync(cardId, 750m, YesterdayUtc(), "Transaction 2");

        var response = await _client.GetAsync(
            $"/api/v1/cards/{cardId}/balance?currency=EUR");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<CardBalanceResponse>();
        Assert.NotNull(body);
        Assert.Equal(cardId, body.CardId);
        Assert.Equal(5000m, body.CreditLimit);
        Assert.Equal(1200m, body.TotalTransactionAmount);
        Assert.Equal(3800m, body.OriginalAvailableBalance);
        Assert.Equal("AUD", body.OriginalCurrency);
        Assert.Equal("EUR", body.TargetCurrency);

        Assert.Equal(2225.60m, body.ConvertedAvailableBalance);
        Assert.Equal(0.585685m, body.ExchangeRateUsed);
    }

    private async Task<Guid> CreateCardAsync(decimal creditLimit)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/cards",
            new CreateCardRequest { CreditLimit = creditLimit });

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<CardResponse>();
        Assert.NotNull(body);

        return body.Id;
    }

    private async Task<Guid> CreateTransactionAsync(
        Guid cardId,
        decimal amount,
        DateOnly transactionDate,
        string description)
    {
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/cards/{cardId}/transactions",
            new TransactionRequest
            {
                Description = description,
                Amount = amount,
                TransactionDate = transactionDate
            });

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<TransactionResponse>();
        Assert.NotNull(body);

        return body.Id;
    }

    private static DateOnly YesterdayUtc()
    {
        return DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
    }
}