using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.DbAccess;
using L_Bank_W_Backend.DbAccess.Data;
using L_Bank_W_Backend.DbAccess.Repositories;
using L_Bank_W_Backend.DbAccess.Util;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

public class LedgerRepositoryTests : IDisposable
{
    private readonly Mock<IOptions<DatabaseSettings>> _databaseSettingsMock = new();
    private readonly Mock<ILogger<LedgerRepository>> _loggerMock = new();
    private readonly Mock<CustomTransactionManager> _transactionManagerMock = new();
    private readonly AppDbContext _context;
    private readonly SqliteConnection _connection;

    public LedgerRepositoryTests()
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

    private LedgerRepository CreateTestee()
    {
        return new LedgerRepository(_databaseSettingsMock.Object, _context, _transactionManagerMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllLedgers_ReturnsOrderedLedgers()
    {
        // Arrange
        var repository = CreateTestee();

        _context.Ledgers.AddRange(new Ledger { Name = "B" }, new Ledger { Name = "A" });
        await _context.SaveChangesAsync();

        // Act
        var result = await repository.GetAllLedgers(CancellationToken.None);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Equal("A", resultList[0].Name);
    }

    [Fact]
    public async Task GetAllLedgers_CancellationTokenIsRespected()
    {
        // Arrange
        var repository = CreateTestee();

        using var cancellationTokenSource = new CancellationTokenSource();
        // Act & Assert
        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            repository.GetAllLedgers(cancellationTokenSource.Token));
    }

    [Fact]
    public async Task GetAllLedgers_ReturnsEmptyListWhenDatabaseIsEmpty()
    {
        // Arrange
        var repository = CreateTestee();

        // Act
        var result = await repository.GetAllLedgers(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }


    [Fact]
    public async Task DeleteLedger_DeletesExistingLedger_ReturnsTrue()
    {
        // Arrange
        const int ledgerId = 1;
        var repository = CreateTestee();
        _context.Ledgers.Add(new Ledger { Id = ledgerId });
        await _context.SaveChangesAsync();

        // Act
        var result = await repository.DeleteLedger(ledgerId, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.DoesNotContain(_context.Ledgers, l => l.Id == ledgerId);
    }

    [Fact]
    public async Task DeleteLedger_WithNonExistingLedgerId_ReturnsFalse()
    {
        // Arrange
        const int nonExistingLedgerId = 99;
        var repository = CreateTestee();

        // Act
        var result = await repository.DeleteLedger(nonExistingLedgerId, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CreateLedger_CreatesNewLedger_AssignsId()
    {
        // Arrange
        var repository = CreateTestee();
        const string ledgerName = "Test Ledger";

        // Act
        var newLedgerId = await repository.CreateLedger(ledgerName);

        // Assert
        var ledgerInDb = await _context.Ledgers.FindAsync(newLedgerId);
        Assert.NotNull(ledgerInDb);
        Assert.Equal(ledgerName, ledgerInDb.Name);
        Assert.Equal(0, ledgerInDb.Balance);
    }
}