using System.Data;
using L_Bank_W_Backend.DbAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace L_Bank_W_Backend.DbAccess.Util;

public class TransactionManager(AppDbContext context)
{
    public async Task<TResult> ExecuteTransactionAsync<TResult>(
        Func<AppDbContext, CancellationToken, Task<TResult>> transactionalOperations,
        CancellationToken ct = default)
    {
        var strategy = context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

            try
            {
                var result = await transactionalOperations(context, ct);

                await transaction.CommitAsync(ct);

                return result;
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        });
    }
}
