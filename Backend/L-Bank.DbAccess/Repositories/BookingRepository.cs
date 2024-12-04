using System.Data;
using L_Bank_W_Backend.DbAccess.Data;
using L_Bank_W_Backend.DbAccess.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace L_Bank_W_Backend.DbAccess.Repositories;

public class BookingRepository(AppDbContext dbContext, ILogger<BookingRepository> logger)
    : IBookingRepository
{
    private const int MaxRetries = 20;
    private const IsolationLevel IsolationLevel = System.Data.IsolationLevel.Serializable;

    public async Task<bool> Book(int sourceLedgerId, int destinationLedgerId, decimal amount, CancellationToken ct)
    {

        for (var retries = 0; retries < MaxRetries; retries++)
        {
            IDbContextTransaction? transaction = null;
            try
            {
                transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel, ct);

                var sourceLedger = await dbContext.Ledgers.FindAsync([sourceLedgerId], ct);
                var destinationLedger = await dbContext.Ledgers.FindAsync([destinationLedgerId], ct);

                if (sourceLedger == null || destinationLedger == null)
                {
                    logger.LogWarning(
                        "Book operation failed with source '{sourceLedgerId}' and destination '{destinationLedgerId}', ledger not found.",
                        sourceLedgerId, destinationLedgerId);
                    await DatabaseUtil.RollbackAndDisposeTransactionAsync(transaction, ct);
                    return false;
                }

                if (sourceLedger.Balance < amount)
                {
                    logger.LogWarning(
                        "Book operation failed with source '{sourceLedgerId}' and destination '{destinationLedgerId}', insufficient balance.",
                        sourceLedgerId, destinationLedgerId);
                    await DatabaseUtil.RollbackAndDisposeTransactionAsync(transaction, ct);
                    return false;
                }

                sourceLedger.Balance -= amount;
                destinationLedger.Balance += amount;

                await dbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return true;
            }
            catch (DbUpdateConcurrencyException ex) when (DatabaseUtil.IsDeadlock(ex))
            {
                logger.LogWarning(ex, "Retry '{RetryCount}' of '{MaxRetries}' due to deadlock.", retries + 1,
                    MaxRetries);

                // On deadlock, we perform the retry after a delay based on the retry count.
                await DatabaseUtil.RollbackAndDisposeTransactionAsync(transaction, ct);
                await Task.Delay(DatabaseUtil.ComputeExponentialBackoff(retries), ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An exception occurred on retry '{RetryCount}' of '{MaxRetries}'.", retries + 1,
                    MaxRetries);

                await DatabaseUtil.RollbackAndDisposeTransactionAsync(transaction, ct);

                if (retries >= MaxRetries - 1) throw;
                await Task.Delay(DatabaseUtil.ComputeExponentialBackoff(retries), ct);
            }
        }

        return false;
    }
}