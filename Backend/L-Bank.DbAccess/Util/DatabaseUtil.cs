using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace L_Bank_W_Backend.DbAccess.Util;

public static class DatabaseUtil
{
    public static int ComputeExponentialBackoff(int retries)
    {
        return (int)(Math.Pow(2, retries) * 100);
    }

    public static bool IsDeadlock(DbUpdateConcurrencyException ex)
    {
        var innerException = ex.InnerException as SqlException;
        return innerException is { Number: 1205 };
    }

    public static async Task RollbackAndDisposeTransactionAsync(IDbContextTransaction? transaction,
        CancellationToken ct)
    {
        if (transaction == null) return;

        await transaction.RollbackAsync(ct);
        await transaction.DisposeAsync();
    }
}