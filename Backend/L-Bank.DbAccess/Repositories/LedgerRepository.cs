using System.Data;
using System.Data.SqlClient;
using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.DbAccess.Data;
using L_Bank_W_Backend.DbAccess.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace L_Bank_W_Backend.DbAccess.Repositories;

public class LedgerRepository(
    IOptions<DatabaseSettings> databaseSettings,
    AppDbContext context,
    ILogger<LedgerRepository> logger)
    : ILedgerRepository
{
    private const int MaxRetries = 10;

    private readonly DatabaseSettings _databaseSettings = databaseSettings.Value;

    public decimal GetTotalMoney()
    {
        const string query = @$"SELECT SUM(balance) AS TotalBalance FROM {Ledger.CollectionName}";
        decimal totalBalance = 0;

        using var conn = new SqlConnection(_databaseSettings.ConnectionString);
        conn.Open();
        using var cmd = new SqlCommand(query, conn);
        var result = cmd.ExecuteScalar();
        if (result != DBNull.Value)
        {
            totalBalance = Convert.ToDecimal(result);
        }

        return totalBalance;
    }

    public async Task<IEnumerable<Ledger>> GetAllLedgers(CancellationToken ct)
    {
        var transactionManager = new TransactionManager(context);
        return await transactionManager.ExecuteTransactionAsync(GetLedgersTransactionAsync, ct);
    }

    private static async Task<IEnumerable<Ledger>> GetLedgersTransactionAsync(AppDbContext context,
        CancellationToken ct)
    {
        return await context.Ledgers
            .AsNoTracking()
            .OrderBy(ledger => ledger.Name)
            .ToListAsync(ct);
    }

    public async Task<bool> DeleteLedger(int id, CancellationToken ct)
    {
        var transactionManager = new TransactionManager(context);
        var result = await transactionManager.ExecuteTransactionAsync<bool>(
            async (ctx, token) =>
            {
                var ledgerToDelete = await ctx.Ledgers.FindAsync(new object[] { id }, token);

                if (ledgerToDelete == null)
                {
                    logger.LogInformation("Ledger with id {Id} not found.", id);
                    return false;
                }

                ctx.Ledgers.Remove(ledgerToDelete);
                await ctx.SaveChangesAsync(token);

                return true;
            },
            ct
        );

        if (!result)
        {
            logger.LogWarning("Could not delete the ledger with id {Id}.", id);
        }

        return result;
    }

    public async Task<Ledger?> SelectOneAsync(int id, CancellationToken ct = default)
    {
        for (var retries = 0; retries < MaxRetries; retries++)
        {
            try
            {
                return await context.Ledgers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(l => l.Id == id, ct);
            }
            catch (DbUpdateConcurrencyException ex) when (DatabaseUtil.IsDeadlock(ex))
            {
                logger.LogWarning(ex,
                    "Deadlock detected when trying to access ledger with ID {Id}, attempt {RetryCount}.", id,
                    retries + 1);
                await Task.Delay(DatabaseUtil.ComputeExponentialBackoff(retries), ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "An error occurred when trying to access ledger with ID {Id}, attempt {RetryCount}.", id,
                    retries + 1);
                if (retries >= MaxRetries - 1) throw;
                await Task.Delay(DatabaseUtil.ComputeExponentialBackoff(retries), ct);
            }
        }

        logger.LogCritical("Select operation failed to find ledger with ID {Id} after {MaxRetries} retries.", id,
            MaxRetries);
        throw new Exception($"Select operation failed after {MaxRetries} retries.");
    }

    public Ledger SelectOne(int id, SqlConnection conn, SqlTransaction? transaction)
    {
        const string query = @$"SELECT id, name, balance FROM {Ledger.CollectionName} WHERE id=@Id";

        using var cmd = new SqlCommand(query, conn, transaction);
        cmd.Parameters.AddWithValue("@Id", id);
        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
            throw new Exception($"No Ledger with id {id}");

        var ordId = reader.GetInt32(reader.GetOrdinal("id"));
        var ordName = reader.GetString(reader.GetOrdinal("name"));
        var ordBalance = reader.GetDecimal(reader.GetOrdinal("balance"));

        var retLedger = new Ledger { Id = ordId, Name = ordName, Balance = ordBalance };

        return retLedger;
    }

    public void Update(Ledger ledger, SqlConnection conn, SqlTransaction? transaction)
    {
        const string query = $"UPDATE {Ledger.CollectionName} SET name=@Name, balance=@Balance WHERE id=@Id";
        using var cmd = new SqlCommand(query, conn, transaction);
        cmd.Parameters.AddWithValue("@Name", ledger.Name);
        cmd.Parameters.AddWithValue("@Balance", ledger.Balance);
        cmd.Parameters.AddWithValue("@Id", ledger.Id);

        // Execute the command
        cmd.ExecuteNonQuery();
    }

    public async Task<int> CreateLedger(string name)
    {
        var newLedger = new Ledger
        {
            Name = name,
            Balance = 0,
        };

        var ledger = await context.Ledgers.AddAsync(newLedger);
        await context.SaveChangesAsync();
        return ledger.Entity.Id;
    }

    public void Update(Ledger ledger)
    {
        using var conn = new SqlConnection(_databaseSettings.ConnectionString);
        conn.Open();
        Update(ledger, conn, null);
    }

    public decimal? GetBalance(int ledgerId, SqlConnection conn, SqlTransaction transaction)
    {
        const string query = @"SELECT balance FROM ledgers WHERE id=@Id";

        using var cmd = new SqlCommand(query, conn, transaction);
        cmd.Parameters.AddWithValue("@Id", ledgerId);
        var result = cmd.ExecuteScalar();
        if (result != DBNull.Value)
        {
            return Convert.ToDecimal(result);
        }

        return null;
    }
}