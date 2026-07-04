using CardTransactions.Business.Interfaces;
using CardTransactions.Business.Services;
using CardTransactions.Contracts.Requests;
using CardTransactions.Data.Entities;
using CardTransactions.Data.Repositories;
using Moq;

namespace CardTransactions.Business.Tests;

public class TransactionServiceTests
{
    private readonly Mock<ICardRepository> _cardRepository = new();
    private readonly Mock<ITransactionRepository> _transactionRepository = new();
    private readonly Mock<ICurrencyConversionService> _currencyConversionService = new();

    private TransactionService CreateService()
    {
        return new TransactionService(
            _cardRepository.Object,
            _transactionRepository.Object,
            _currencyConversionService.Object);
    }

    private static TransactionRequest ValidRequest(DateOnly transactionDate)
    {
        return new TransactionRequest
        {
            Description = "Marriott Hotel Sydney",
            TransactionDate = transactionDate,
            Amount = 450m
        };
    }

    [Fact]
    public async Task CreateTransactionAsync_ValidRequest_ReturnsTransactionResponse()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var request = ValidRequest(DateOnly.FromDateTime(DateTime.UtcNow));

        _cardRepository
            .Setup(x => x.ExistsAsync(cardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CreateService();

        // Act
        var response = await service.CreateTransactionAsync(cardId, request);

        // Assert
        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal(cardId, response.CardId);
        Assert.Equal("Marriott Hotel Sydney", response.Description);
        Assert.Equal(request.TransactionDate, response.TransactionDate);
        Assert.Equal(450m, response.Amount);
        Assert.Equal("AUD", response.Currency);

        _transactionRepository.Verify(
            x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _transactionRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateTransactionAsync_CardNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var cardId = Guid.NewGuid();
        var request = ValidRequest(DateOnly.FromDateTime(DateTime.UtcNow));

        _cardRepository
            .Setup(x => x.ExistsAsync(cardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var service = CreateService();

        // Act
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.CreateTransactionAsync(cardId, request));

        // Assert
        Assert.Equal($"Card '{cardId}' was not found.", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task CreateTransactionAsync_InvalidAmount_ThrowsArgumentException(decimal amount)
    {
        // Arrange
        var request = ValidRequest(DateOnly.FromDateTime(DateTime.UtcNow));
        request.Amount = amount;

        var service = CreateService();

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateTransactionAsync(Guid.NewGuid(), request));

        // Assert
        Assert.StartsWith("Amount must be greater than 0.", exception.Message);
        Assert.Equal("request", exception.ParamName);

        _transactionRepository.Verify(
            x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateTransactionAsync_EmptyDescription_ThrowsArgumentException()
    {
        // Arrange
        var request = ValidRequest(DateOnly.FromDateTime(DateTime.UtcNow));
        request.Description = "   ";

        var service = CreateService();

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateTransactionAsync(Guid.NewGuid(), request));

        // Assert
        Assert.StartsWith("Description is required.", exception.Message);
        Assert.Equal("request", exception.ParamName);
    }

    [Fact]
    public async Task CreateTransactionAsync_FutureTransactionDate_ThrowsArgumentException()
    {
        // Arrange
        var request = ValidRequest(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));
        var service = CreateService();

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateTransactionAsync(Guid.NewGuid(), request));

        // Assert
        Assert.StartsWith("TransactionDate cannot be in the future.", exception.Message);
        Assert.Equal("request", exception.ParamName);

        _cardRepository.Verify(
            x => x.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}