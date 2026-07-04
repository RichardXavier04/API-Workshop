using CardTransactions.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CardTransactions.Data.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly CardTransactionsDbContext _context;

    public TransactionRepository(CardTransactionsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        await _context.Transactions.AddAsync(transaction, cancellationToken);
    }

    public async Task<Transaction?> GetByIdAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == transactionId, cancellationToken);
    }

    public async Task<decimal> GetTotalAmountByCardIdAsync(Guid cardId, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Where(x => x.CardId == cardId)
            .SumAsync(x => x.Amount, cancellationToken);
    }
    
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
    
}