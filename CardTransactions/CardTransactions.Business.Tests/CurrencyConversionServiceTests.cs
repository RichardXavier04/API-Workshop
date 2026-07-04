using CardTransactions.Business.Exceptions;
using CardTransactions.Business.Services;
using CardTransactions.Data.Clients;
using Moq;

namespace CardTransactions.Business.Tests;

public class CurrencyConversionServiceTests
{
    private readonly Mock<ITreasuryExchangeRateClient> _treasuryClient = new();

    private CurrencyConversionService CreateService()
    {
        return new CurrencyConversionService(_treasuryClient.Object);
    }

    [Fact]
    public async Task ConvertAsync_SameCurrency_ReturnsRateOneAndSameAmount()
    {
        // Arrange
        var service = CreateService();

        // Act
        var (exchangeRateUsed, convertedAmount) = await service.ConvertAsync(
            750m,
            "AUD",
            "AUD",
            new DateOnly(2025, 3, 15));

        // Assert
        Assert.Equal(1m, exchangeRateUsed);
        Assert.Equal(750m, convertedAmount);

        _treasuryClient.Verify(
            x => x.GetRatesAsync(
                It.IsAny<string>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ConvertAsync_HistoricalRateExists_ConvertsAmount()
    {
        // Arrange
        var transactionDate = new DateOnly(2025, 3, 15);
        var windowStart = transactionDate.AddMonths(-6);

        _treasuryClient
            .Setup(x => x.GetRatesAsync(
                "AUD",
                windowStart,
                transactionDate,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TreasuryExchangeRate>
            {
                new()
                {
                    RecordDate = new DateOnly(2024, 12, 31),
                    Currency = "AUD",
                    ExchangeRate = 1.612m
                },
                new()
                {
                    RecordDate = new DateOnly(2026, 5, 31),
                    Currency = "AUD",
                    ExchangeRate = 1.600m
                }
            });

        var service = CreateService();

        // Act
        var (exchangeRateUsed, convertedAmount) = await service.ConvertAsync(
            750m,
            "AUD",
            "USD",
            transactionDate);

        // Assert
        Assert.Equal(0.620347m, exchangeRateUsed);
        Assert.Equal(465.26m, convertedAmount);
    }

    [Fact]
    public async Task ConvertAsync_NoHistoricalRate_ThrowsCurrencyConversionException()
    {
        // Arrange
        _treasuryClient
            .Setup(x => x.GetRatesAsync(
                It.IsAny<string>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<TreasuryExchangeRate>());

        var service = CreateService();

        // Act
        var exception = await Assert.ThrowsAsync<CurrencyConversionException>(() =>
            service.ConvertAsync(
                750m,
                "AUD",
                "USD",
                new DateOnly(2025, 3, 15)));

        // Assert
        Assert.Equal(
            "The transaction cannot be converted to the target currency.",
            exception.Message);
    }

    [Fact]
    public async Task ConvertWithLatestRateAsync_SameCurrency_ReturnsRateOneAndSameAmount()
    {
        // Arrange
        var service = CreateService();

        // Act
        var (exchangeRateUsed, convertedAmount) = await service.ConvertWithLatestRateAsync(
            3050m,
            "AUD",
            "AUD");

        // Assert
        Assert.Equal(1m, exchangeRateUsed);
        Assert.Equal(3050m, convertedAmount);

        _treasuryClient.Verify(
            x => x.GetLatestRateAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}