using Microsoft.EntityFrameworkCore;
using CardTransactions.Data.Entities;


namespace CardTransactions.Data.Repositories;

public class CardRepository : ICardRepository
{
    private readonly CardTransactionsDbContext _context;

    public CardRepository(CardTransactionsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Card card, CancellationToken cancellationToken = default)
    {
        await _context.Cards.AddAsync(card, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid cardId, CancellationToken cancellationToken = default)
    {
        return await _context.Cards.AnyAsync(x => x.Id == cardId, cancellationToken);
    }
}