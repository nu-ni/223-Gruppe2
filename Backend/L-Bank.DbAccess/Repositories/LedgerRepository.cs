using System.Data;
using System.Data.SqlClient;
using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.DbAccess.Data;
using L_Bank_W_Backend.DbAccess.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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

        using var conn = new SqlConnection(this._databaseSettings.ConnectionString);
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
        await using var transaction =
            await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken: ct);
        try
        {
            var allLedgers = await context.Ledgers
                .AsNoTracking()
                .OrderBy(ledger => ledger.Name)
                .ToListAsync(cancellationToken: ct);

            await transaction.CommitAsync(ct);

            return allLedgers;
        }
        catch
        {
            await DatabaseUtil.RollbackAndDisposeTransactionAsync(transaction, ct);
            throw;
        }
    }

    public async Task<bool> DeleteLedger(int id, CancellationToken ct)
    {
        var retries = 0;
        IDbContextTransaction? transaction = null;
        const IsolationLevel isolationLevel = IsolationLevel.ReadCommitted;

        while (retries < MaxRetries)
        {
            try
            {
                transaction =
                    await context.Database.BeginTransactionAsync(isolationLevel, ct);
                var ledgerToDelete = await context.Ledgers.FindAsync([id], ct);

                if (ledgerToDelete == null)
                {
                    logger?.LogInformation($"Ledger with id {id} not found.");
                    return false;
                }

                context.Ledgers.Remove(ledgerToDelete);
                await context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                return true;
            }
            catch (SqlException ex) when (ex.Number == 1205)
            {
                retries++;
                logger?.LogWarning(ex,
                    "Deadlock occurred while trying to delete ledger with id {Id}. Retrying {RetryCount} time.", id,
                    retries);
                await DatabaseUtil.RollbackAndDisposeTransactionAsync(transaction, ct);
                await Task.Delay(DatabaseUtil.ComputeExponentialBackoff(retries), ct);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "An exception occurred while trying to delete ledger with id {Id}.", id);
                await DatabaseUtil.RollbackAndDisposeTransactionAsync(transaction, ct);
                throw;
            }
        }

        logger?.LogCritical("Delete operation failed for ledger with id {Id} after {MaxRetries} retries.", id,
            MaxRetries);
        throw new Exception($"Delete operation failed after {MaxRetries} retries.");
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