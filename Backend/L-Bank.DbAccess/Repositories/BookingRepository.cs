using L_Bank_W_Backend.DbAccess.Data;
using L_Bank_W_Backend.DbAccess.Util;

namespace L_Bank_W_Backend.DbAccess.Repositories;

public class BookingRepository(AppDbContext context) : IBookingRepository
{
    private readonly CustomTransactionManager _transactionManager = new(context);

    public async Task<bool> Book(int sourceLedgerId, int destinationLedgerId, decimal amount, CancellationToken ct)
    {
        return await _transactionManager.ExecuteTransactionAsync(
            async (ctx, token) =>
                await ExecuteBookingTransactionAsync(ctx, sourceLedgerId, destinationLedgerId, amount, token),
            ct);
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