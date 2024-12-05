using System.Data;
using L_Bank_W_Backend.DbAccess.Data;
using L_Bank_W_Backend.DbAccess.Util;
using L_Bank.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace L_Bank_W_Backend.DbAccess.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<BookingRepository> _logger;
    private const int MaxRetries = 20;

    public BookingRepository(AppDbContext dbContext, ILogger<BookingRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> Book(int sourceLedgerId, int destinationLedgerId, decimal amount, CancellationToken ct)
    {
        for (var retries = 0; retries < MaxRetries; retries++)
        {
            IDbContextTransaction? transaction = null;
            try
            {
                // Begin a transaction with a high isolation level for safety.
                transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

                // Fetch ledgers using EF Core's tracking.
                var sourceLedger = await _dbContext.Ledgers
                    .Where(l => l.Id == sourceLedgerId)
                    .FirstOrDefaultAsync(ct);

                var destinationLedger = await _dbContext.Ledgers
                    .Where(l => l.Id == destinationLedgerId)
                    .FirstOrDefaultAsync(ct);

                // Validate ledger existence.
                if (sourceLedger == null || destinationLedger == null)
                {
                    _logger.LogWarning(
                        "Booking failed: Ledger not found (Source: {SourceLedgerId}, Destination: {DestinationLedgerId}).",
                        sourceLedgerId, destinationLedgerId);
                    await DatabaseUtil.RollbackAndDisposeTransactionAsync(transaction, ct);
                    return false;
                }

                // Ensure ledgers are tracked by EF.
                _dbContext.Entry(sourceLedger).State = EntityState.Modified;
                _dbContext.Entry(destinationLedger).State = EntityState.Modified;

                // Check if source ledger has sufficient funds.
                if (sourceLedger.Balance < amount)
                {
                    _logger.LogWarning(
                        "Booking failed: Insufficient funds in Source Ledger (Source: {SourceLedgerId}, Balance: {Balance}, Amount: {Amount}).",
                        sourceLedgerId, sourceLedger.Balance, amount);
                    await DatabaseUtil.RollbackAndDisposeTransactionAsync(transaction, ct);
                    return false;
                }

                // Perform balance updates with rounding to avoid precision issues.
                sourceLedger.Balance = Math.Round(sourceLedger.Balance - amount, 2);
                destinationLedger.Balance = Math.Round(destinationLedger.Balance + amount, 2);

                // Create and save the booking transaction.
                var booking = new Booking
                {
                    SourceId = sourceLedgerId,
                    DestinationId = destinationLedgerId,
                    Amount = amount
                };

                await _dbContext.Bookings.AddAsync(booking, ct);

                // Save changes and commit the transaction.
                await _dbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                return true;
            }
            catch (DbUpdateConcurrencyException ex) when (DatabaseUtil.IsDeadlock(ex))
            {
                _logger.LogWarning(
                    ex, "Deadlock detected. Retrying ({RetryCount}/{MaxRetries}).", retries + 1, MaxRetries);

                await DatabaseUtil.RollbackAndDisposeTransactionAsync(transaction, ct);
                await Task.Delay(DatabaseUtil.ComputeExponentialBackoff(retries), ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "Exception occurred during booking (Retry {RetryCount}/{MaxRetries}).", retries + 1, MaxRetries);

                await DatabaseUtil.RollbackAndDisposeTransactionAsync(transaction, ct);

                if (retries >= MaxRetries - 1) throw;
                await Task.Delay(DatabaseUtil.ComputeExponentialBackoff(retries), ct);
            }
        }

        return false;
    }

    public async Task<IEnumerable<object>> GetBookingHistory(CancellationToken ct)
    {
        var history = await _dbContext.Bookings
            .AsNoTracking()
            .Select(b => new
            {
                b.Id,
                b.SourceId,
                b.DestinationId,
                b.Amount,
                Source = _dbContext.Ledgers
                    .Where(l => l.Id == b.SourceId)
                    .Select(l => l.Name)
                    .FirstOrDefault(),
                Destination = _dbContext.Ledgers
                    .Where(l => l.Id == b.DestinationId)
                    .Select(l => l.Name)
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        return history.Cast<object>();
    }
}
