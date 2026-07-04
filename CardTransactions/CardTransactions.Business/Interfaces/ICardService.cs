using CardTransactions.Contracts.Requests;
using CardTransactions.Contracts.Responses;

namespace CardTransactions.Business.Interfaces;

public interface ICardService
{
    Task<CardResponse> CreateCardAsync(
        CreateCardRequest request,
        CancellationToken cancellationToken = default);

    Task<CardBalanceResponse> GetBalanceInCurrencyAsync(
    Guid cardId,
    string currency,
    CancellationToken cancellationToken = default);
}