using System.Data.SqlClient;
using L_Bank_W_Backend.Core.Models;

namespace L_Bank_W_Backend.DbAccess.Repositories;

public interface ILedgerRepository
{
    Task<IEnumerable<Ledger>> GetAllLedgers(CancellationToken ct);
    decimal GetTotalMoney();
    Task<Ledger?> SelectOneAsync(int id, CancellationToken ct);
    Ledger? SelectOne(int id, SqlConnection conn, SqlTransaction? transaction);
    Task<int> CreateLedger(string name);
    void Update(Ledger ledger, SqlConnection conn, SqlTransaction transaction);
    void Update(Ledger ledger);
    decimal? GetBalance(int ledgerId, SqlConnection conn, SqlTransaction transaction);
    Task<bool> DeleteLedger(int id, CancellationToken ct);
}