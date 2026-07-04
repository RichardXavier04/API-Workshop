using CardTransactions.Business.Interfaces;
using CardTransactions.Business.Services;
using CardTransactions.Data.Entities;
using CardTransactions.Data.Repositories;
using Moq;

namespace CardTransactions.Business.Tests;

public class CardBalanceServiceTests
{
    private readonly Mock<ICardRepository> _cardRepository = new();
    private readonly Mock<ITransactionRepository> _transactionRepository = new();
    private readonly Mock<ICurrencyConversionService> _currencyConversionService = new();

    private CardService CreateService()
    {
        return new CardService(
            _cardRepository.Object,
            _transactionRepository.Object,
            _currencyConversionService.Object);
    }

    [Fact]
    public async Task GetBalanceInCurrencyAsync_TransactionsExist_ReturnsAvailableBalance()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var card = new Card
        {
            Id = cardId,
            CreditLimit = 5000m,
            Currency = "AUD"
        };

        _cardRepository
            .Setup(x => x.GetByIdAsync(cardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(card);

        _transactionRepository
            .Setup(x => x.GetTotalAmountByCardIdAsync(cardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1950m);

        _currencyConversionService
            .Setup(x => x.ConvertWithLatestRateAsync(
                3050m,
                "AUD",
                "AUD",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((1m, 3050m));

        var service = CreateService();

        // Act
        var response = await service.GetBalanceInCurrencyAsync(cardId, "AUD");

        // Assert
        Assert.Equal(cardId, response.CardId);
        Assert.Equal(5000m, response.CreditLimit);
        Assert.Equal(1950m, response.TotalTransactionAmount);
        Assert.Equal(3050m, response.OriginalAvailableBalance);
        Assert.Equal("AUD", response.OriginalCurrency);
        Assert.Equal("AUD", response.TargetCurrency);
        Assert.Equal(1m, response.ExchangeRateUsed);
        Assert.Equal(3050m, response.ConvertedAvailableBalance);
    }

    [Fact]
    public async Task GetBalanceInCurrencyAsync_CardNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var cardId = Guid.NewGuid();

        _cardRepository
            .Setup(x => x.GetByIdAsync(cardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Card?)null);

        var service = CreateService();

        // Act
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.GetBalanceInCurrencyAsync(cardId, "AUD"));

        // Assert
        Assert.Equal($"Card '{cardId}' was not found.", exception.Message);
    }

    [Fact]
    public async Task GetBalanceInCurrencyAsync_MissingCurrency_ThrowsArgumentException()
    {
        // Arrange
        var service = CreateService();

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetBalanceInCurrencyAsync(Guid.NewGuid(), "   "));

        // Assert
        Assert.StartsWith("Currency is required.", exception.Message);
        Assert.Equal("currency", exception.ParamName);
    }
}