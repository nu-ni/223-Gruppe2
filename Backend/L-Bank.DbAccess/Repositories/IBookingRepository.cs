using L_Bank.Core.Models;

namespace L_Bank_W_Backend.DbAccess.Repositories;

public interface IBookingRepository
{
    Task<bool> Book(int sourceLedgerId, int destinationLedgerId, decimal amount, CancellationToken ct);
    Task<IEnumerable<object>> GetBookingHistory(CancellationToken ct); // Corrected return type
}
