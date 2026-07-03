using CardTransactions.Contracts.Requests;
using CardTransactions.Contracts.Responses;

namespace CardTransactions.Business.Interfaces;

public interface ITransactionService
{
    Task<TransactionResponse> CreateTransactionAsync(
        Guid cardId,
        TransactionRequest request,
        CancellationToken cancellationToken = default);

    Task<ConvertedTransactionResponse> GetTransactionInCurrencyAsync(
        Guid transactionId,
        string currency,
        CancellationToken cancellationToken = default);
}