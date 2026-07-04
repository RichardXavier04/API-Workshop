using CardTransactions.Data.Entities;

namespace CardTransactions.Data.Repositories;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);

    Task<Transaction?> GetByIdAsync(Guid transactionId, CancellationToken cancellationToken = default);

    Task<decimal> GetTotalAmountByCardIdAsync(Guid cardId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}