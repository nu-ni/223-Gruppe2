using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.DbAccess;
using L_Bank_W_Backend.DbAccess.Data;
using L_Bank_W_Backend.DbAccess.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

public class BookingRepositoryTests : IDisposable
{
    private readonly Mock<ILogger<BookingRepository>> _loggerMock = new();
    private readonly AppDbContext _context;
    private readonly SqliteConnection _connection;

    public BookingRepositoryTests()
    {
        Mock<IOptions<DatabaseSettings>> databaseSettingsMock = new();
        databaseSettingsMock.Setup(x => x.Value).Returns(new DatabaseSettings
        {
            ConnectionString =
                "Server=localhost,1433;Database=l_bank_backend;User Id=sa;Password=YourPassword123;TrustServerCertificate=True;"
        });

        _loggerMock = new Mock<ILogger<BookingRepository>>();
    }

    [Fact]
    public async Task Book_WithValidLedgers_TransfersFundsAndReturnsTrue()
    {
        // Arrange
        var connectionString =
            "Server=localhost,1433;Database=l_bank_backend;User Id=sa;Password=YourPassword123;TrustServerCertificate=True;";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        // Ensure the database is created
        await using (var context = new AppDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();
        }

        const decimal transferAmount = 50m;
        int sourceLedgerId;
        int destinationLedgerId;

        await using (var context = new AppDbContext(options))
        {
            var sourceLedger = new Ledger { Balance = 100m };
            var destinationLedger = new Ledger { Balance = 100m };

            context.Ledgers.AddRange(sourceLedger, destinationLedger);
            await context.SaveChangesAsync();

            sourceLedgerId = sourceLedger.Id;
            destinationLedgerId = destinationLedger.Id;
        }

        // Act
        bool result;
        await using (var context = new AppDbContext(options))
        {
            var repository = new BookingRepository(context, _loggerMock.Object);
            result = await repository.Book(sourceLedgerId, destinationLedgerId, transferAmount,
                CancellationToken.None);
        }

        // Assert
        await using (var context = new AppDbContext(options))
        {
            // Retrieve the ledgers to confirm the balances have been updated
            var sourceLedger = await context.Ledgers.FindAsync(sourceLedgerId);
            var destinationLedger = await context.Ledgers.FindAsync(destinationLedgerId);

            Assert.True(result);
            Assert.NotNull(sourceLedger);
            Assert.NotNull(destinationLedger);
            Assert.Equal(50m, sourceLedger.Balance);
            Assert.Equal(150m,
                destinationLedger.Balance);
        }
    }

    [Fact]
    public async Task Book_WithNonExistingSourceLedger_ReturnsFalse()
    {
        // Arrange
        const int nonExistingLedgerId = 99;
        const int destinationLedgerId = 2;
        const decimal transferAmount = 50m;

        // Arrange
        var connectionString =
            "Server=localhost,1433;Database=l_bank_backend;User Id=sa;Password=YourPassword123;TrustServerCertificate=True;";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        // Ensure the database is created
        await using (var context = new AppDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();
        }

        // Act
        bool result;
        await using (var context = new AppDbContext(options))
        {
            var repository = new BookingRepository(context, _loggerMock.Object);
            result = await repository.Book(nonExistingLedgerId , destinationLedgerId, transferAmount,
                CancellationToken.None);
        }

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task Book_WithInsufficientBalance_ReturnsFalse()
    {
        // Arrange
        const decimal transferAmount = 150m;

        // Arrange
        var connectionString =
            "Server=localhost,1433;Database=l_bank_backend;User Id=sa;Password=YourPassword123;TrustServerCertificate=True;";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;


        int sourceLedgerId;
        int destinationLedgerId;

        await using (var context = new AppDbContext(options))
        {
            var sourceLedger = new Ledger { Balance = 100m };
            var destinationLedger = new Ledger { Balance = 100m };

            context.Ledgers.AddRange(sourceLedger, destinationLedger);
            await context.SaveChangesAsync();

            sourceLedgerId = sourceLedger.Id;
            destinationLedgerId = destinationLedger.Id;
        }

        // Ensure the database is created
        await using (var context = new AppDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();
        }

        // Act
        bool result;
        await using (var context = new AppDbContext(options))
        {
            var repository = new BookingRepository(context, _loggerMock.Object);
            result = await repository.Book(sourceLedgerId , destinationLedgerId, transferAmount,
                CancellationToken.None);
        }

        // Assert
        Assert.False(result);
    }
}