namespace L_Bank_W_Backend.DbAccess.Repositories;

public interface IBookingRepository
{
    Task<bool> Book(int sourceLedgerId, int destinationLedgerId, decimal amount, CancellationToken ct);
}