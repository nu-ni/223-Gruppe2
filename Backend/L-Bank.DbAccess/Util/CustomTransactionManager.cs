using System.Data;
using L_Bank_W_Backend.DbAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace L_Bank_W_Backend.DbAccess.Util;

public class CustomTransactionManager
{
    private readonly AppDbContext _context;

    public CustomTransactionManager(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TResult> ExecuteTransactionAsync<TResult>(
        Func<AppDbContext, CancellationToken, Task<TResult>> transactionalOperations,
        CancellationToken ct = default)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

            try
            {
                var result = await transactionalOperations(_context, ct);

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
