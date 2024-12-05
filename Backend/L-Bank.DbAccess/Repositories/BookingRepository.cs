using System.Transactions;
using L_Bank_W_Backend.DbAccess.Data;
using L_Bank_W_Backend.DbAccess.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TransactionManager = L_Bank_W_Backend.DbAccess.Util.TransactionManager;

namespace L_Bank_W_Backend.DbAccess.Repositories;

public class BookingRepository(AppDbContext context, ILogger<BookingRepository> logger) : IBookingRepository
{
    public async Task<bool> Book(int sourceLedgerId, int destinationLedgerId, decimal amount, CancellationToken ct)
    {
        try
        {
            var transactionManager = new TransactionManager(context);

            return await transactionManager.ExecuteTransactionAsync(
                async (ctx, token) => await ExecuteBookingTransactionAsync(ctx, sourceLedgerId, destinationLedgerId, amount, token),
                ct);
        }
        catch (DbUpdateConcurrencyException ex) when (DatabaseUtil.IsDeadlock(ex))
        {
            logger.LogError(ex, "A deadlock occurred during the booking operation.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occurred during the booking operation.");
        }

        return false;
    }

    private static async Task<bool> ExecuteBookingTransactionAsync(
        AppDbContext ctx,
        int sourceLedgerId,
        int destinationLedgerId,
        decimal amount,
        CancellationToken ct)
    {
        var sourceLedger = await ctx.Ledgers.FindAsync([sourceLedgerId], ct);
        var destinationLedger = await ctx.Ledgers.FindAsync([destinationLedgerId], ct);

        if (sourceLedger == null || destinationLedger == null)
        {
            throw new InvalidOperationException("Ledger not found.");
        }

        if (sourceLedger.Balance < amount)
        {
            throw new InvalidOperationException("Insufficient balance.");
        }

        sourceLedger.Balance -= amount;
        destinationLedger.Balance += amount;

        await ctx.SaveChangesAsync(ct);
        return true;
    }
}
