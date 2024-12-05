using System.Data;
using L_Bank_W_Backend.DbAccess.Data;
using L_Bank_W_Backend.DbAccess.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace L_Bank_W_Backend.DbAccess.Repositories;

public class BookingRepository(AppDbContext dbContext, ILogger<BookingRepository> logger)
    : IBookingRepository
{
    private const int MaxRetries = 20;

    public async Task<bool> Book(int sourceLedgerId, int destinationLedgerId, decimal amount, CancellationToken ct)
    {
        for (var retries = 0; retries < MaxRetries; retries++)
        {
            try
            {

                await using var transaction =
                    await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

                var sourceLedger = await dbContext.Ledgers
                    .Where(l => l.Id == sourceLedgerId)
                    .FirstOrDefaultAsync(ct);

                var destinationLedger = await dbContext.Ledgers
                    .Where(l => l.Id == destinationLedgerId)
                    .FirstOrDefaultAsync(ct);

                if (sourceLedger == null || destinationLedger == null)
                {
                    logger.LogWarning(
                        "Book operation failed with source '{sourceLedgerId}' and destination '{destinationLedgerId}', ledger not found.",
                        sourceLedgerId, destinationLedgerId);
                    return false;
                }

                if (sourceLedger.Balance < amount)
                {
                    logger.LogWarning(
                        "Book operation failed with source '{sourceLedgerId}' and destination '{destinationLedgerId}', insufficient balance.",
                        sourceLedgerId, destinationLedgerId);
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

                await Task.Delay(DatabaseUtil.ComputeExponentialBackoff(retries), ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An exception occurred on retry '{RetryCount}' of '{MaxRetries}'.", retries + 1,
                    MaxRetries);

                if (retries >= MaxRetries - 1) throw;
                await Task.Delay(DatabaseUtil.ComputeExponentialBackoff(retries), ct);
            }
        }

        return false;
    }
}