using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.DbAccess.Data;
using L_Bank_W_Backend.DbAccess.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

public class BookingRepositoryTests : IDisposable
{
    private readonly Mock<ILogger<BookingRepository>> _loggerMock = new();
    private readonly AppDbContext _context;
    private readonly SqliteConnection _connection;

    public BookingRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Close();
        _connection.Dispose();
    }

    private BookingRepository CreateTestee()
    {
        return new BookingRepository(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task Book_WithValidLedgers_TransfersFundsAndReturnsTrue()
    {
        // Arrange
        const int sourceLedgerId = 1;
        const int destinationLedgerId = 2;
        const decimal transferAmount = 50m;

        _context.Ledgers.AddRange(
            new Ledger { Id = sourceLedgerId, Balance = 100m },
            new Ledger { Id = destinationLedgerId, Balance = 100m }
        );
        await _context.SaveChangesAsync();

        var repository = CreateTestee();

        // Act
        var result = await repository.Book(sourceLedgerId, destinationLedgerId, transferAmount, CancellationToken.None);
        var sourceLedger = await _context.Ledgers.FindAsync(sourceLedgerId);
        var destinationLedger = await _context.Ledgers.FindAsync(destinationLedgerId);

        // Assert
        Assert.True(result);
        Assert.NotNull(sourceLedger);
        Assert.NotNull(destinationLedger);
        Assert.Equal(50m, sourceLedger.Balance);
        Assert.Equal(150m, destinationLedger.Balance);
    }

    [Fact]
    public async Task Book_WithNonExistingSourceLedger_ReturnsFalse()
    {
        // Arrange
        const int nonExistingLedgerId = 99;
        const int destinationLedgerId = 2;
        const decimal transferAmount = 50m;

        var repository = CreateTestee();

        // Act
        var result = await repository.Book(nonExistingLedgerId, destinationLedgerId, transferAmount, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Book_WithInsufficientBalance_ReturnsFalse()
    {
        // Arrange
        const int sourceLedgerId = 1;
        const int destinationLedgerId = 2;
        const decimal transferAmount = 150m;

        _context.Ledgers.AddRange(
            new Ledger { Id = sourceLedgerId, Balance = 100m },
            new Ledger { Id = destinationLedgerId, Balance = 100m }
        );
        await _context.SaveChangesAsync();

        var repository = CreateTestee();

        // Act
        var result = await repository.Book(sourceLedgerId, destinationLedgerId, transferAmount, CancellationToken.None);

        // Assert
        Assert.False(result);
    }
}