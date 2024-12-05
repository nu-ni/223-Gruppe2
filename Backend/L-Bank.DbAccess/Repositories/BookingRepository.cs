using System.Data;
using L_Bank_W_Backend.DbAccess.Data;
using L_Bank_W_Backend.DbAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<BookingRepository> _logger;

    public BookingRepository(AppDbContext dbContext, ILogger<BookingRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> Book(int sourceLedgerId, int destinationLedgerId, decimal amount, CancellationToken ct)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        try
        {
            // Fetch and track ledgers with EF tracking.
            var sourceLedger = await _dbContext.Ledgers
                .Where(l => l.Id == sourceLedgerId)
                .FirstOrDefaultAsync(ct);

            var destinationLedger = await _dbContext.Ledgers
                .Where(l => l.Id == destinationLedgerId)
                .FirstOrDefaultAsync(ct);

            // Ensure ledgers are tracked by EF.
            _dbContext.Entry(sourceLedger).State = EntityState.Modified;
            _dbContext.Entry(destinationLedger).State = EntityState.Modified;

            // Validation checks.
            if (sourceLedger == null || destinationLedger == null)
            {
                _logger.LogError("One or both ledgers not found.");
                return false;
            }

            if (sourceLedger.Balance < amount)
            {
                _logger.LogError("Insufficient funds in source ledger.");
                return false;
            }

            // Update balances and round to avoid precision issues.
            sourceLedger.Balance = Math.Round(sourceLedger.Balance - amount, 2);
            destinationLedger.Balance = Math.Round(destinationLedger.Balance + amount, 2);

            // Save changes.
            await _dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during booking transaction.");
            await transaction.RollbackAsync(ct);
            return false;
        }
    }
}