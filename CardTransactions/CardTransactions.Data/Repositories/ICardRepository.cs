using CardTransactions.Data.Entities;

namespace CardTransactions.Data.Repositories;

public interface ICardRepository
{
    Task AddAsync(Card card, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid cardId, CancellationToken cancellationToken = default);
    Task<Card?> GetByIdAsync(Guid cardId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}