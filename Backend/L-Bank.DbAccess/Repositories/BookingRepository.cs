using System.Data;
using L_Bank_W_Backend.DbAccess.Data;
using L_Bank_W_Backend.DbAccess.Repositories;
using L_Bank_W_Backend.DbAccess.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<BookingRepository> _logger;
    private const int MaxRetries = 5;

    public BookingRepository(AppDbContext dbContext, ILogger<BookingRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

public async Task<bool> Book(int sourceLedgerId, int destinationLedgerId, decimal amount, CancellationToken ct)
{
    for (var retries = 0; retries < MaxRetries; retries++)
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

            // Validation checks.
            if (sourceLedger == null || destinationLedger == null)
            {
                _logger.LogError("One or both ledgers not found.");
                return false;
            }

            // Ensure ledgers are tracked by EF.
            _dbContext.Entry(sourceLedger).State = EntityState.Modified;
            _dbContext.Entry(destinationLedger).State = EntityState.Modified;

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
        catch (DbUpdateConcurrencyException ex) when (DatabaseUtil.IsDeadlock(ex))
        {
            _logger.LogWarning(ex, "Retry {RetryCount} of {MaxRetries} due to deadlock.", retries + 1, MaxRetries);
            await Task.Delay(DatabaseUtil.ComputeExponentialBackoff(retries), ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred on retry {RetryCount} of {MaxRetries}.", retries + 1, MaxRetries);
            if (retries >= MaxRetries - 1) throw;
            await Task.Delay(DatabaseUtil.ComputeExponentialBackoff(retries), ct);
        }
    }

    return false;
}}