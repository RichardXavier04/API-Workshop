using CardTransactions.Business.Interfaces;
using CardTransactions.Business.Services;
using CardTransactions.Contracts.Requests;
using CardTransactions.Data.Entities;
using CardTransactions.Data.Repositories;
using Moq;

namespace CardTransactions.Business.Tests;

public class CardServiceTests
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
    public async Task CreateCardAsync_ValidRequest_ReturnsCardResponse()
    {
        // Arrange
        var service = CreateService();
        var request = new CreateCardRequest
        {
            CreditLimit = 5000m
        };

        // Act
        var response = await service.CreateCardAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal(5000m, response.CreditLimit);
        Assert.Equal("AUD", response.Currency);

        _cardRepository.Verify(
            x => x.AddAsync(It.IsAny<Card>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _cardRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task CreateCardAsync_InvalidCreditLimit_ThrowsArgumentException(decimal creditLimit)
    {
        // Arrange
        var service = CreateService();
        var request = new CreateCardRequest
        {
            CreditLimit = creditLimit
        };

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateCardAsync(request));

        // Assert
        Assert.StartsWith("CreditLimit must be greater than 0.", exception.Message);

        Assert.Equal("request", exception.ParamName);

        _cardRepository.Verify(
            x => x.AddAsync(It.IsAny<Card>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}