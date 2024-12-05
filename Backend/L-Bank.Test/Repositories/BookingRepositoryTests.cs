using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.DbAccess;
using L_Bank_W_Backend.DbAccess.Data;
using L_Bank_W_Backend.DbAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

public class BookingRepositoryTests
{
    private readonly Mock<IOptions<DatabaseSettings>> _databaseSettingsMock;
    private readonly Mock<ILogger<BookingRepository>> _loggerMock;

    public BookingRepositoryTests()
    {
        _databaseSettingsMock = new Mock<IOptions<DatabaseSettings>>();
        _databaseSettingsMock.Setup(x => x.Value).Returns(new DatabaseSettings
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

        // Initialize ledgers without setting Ids
        await using (var context = new AppDbContext(options))
        {
            var sourceLedger = new Ledger { Balance = 100m };
            var destinationLedger = new Ledger { Balance = 100m };

            context.Ledgers.AddRange(sourceLedger, destinationLedger);
            await context.SaveChangesAsync();

            // After saving, Entity Framework should automatically populate the Id properties with generated values
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
            Assert.Equal(50m, sourceLedger.Balance); // Source ledger balance should be deducted by transferAmount
            Assert.Equal(150m,
                destinationLedger.Balance); // Destination ledger balance should be increased by transferAmount
        }
    }

    // [Fact]
    // public async Task Book_WithNonExistingSourceLedger_ReturnsFalse()
    // {
    //     // Arrange
    //     const int nonExistingLedgerId = 99;
    //     const int destinationLedgerId = 2;
    //     const decimal transferAmount = 50m;
    //
    //     var repository = CreateTestee();
    //
    //     // Act
    //     var result = await repository.Book(nonExistingLedgerId, destinationLedgerId, transferAmount,
    //         CancellationToken.None);
    //
    //     // Assert
    //     Assert.False(result);
    // }
    //
    // [Fact]
    // public async Task Book_WithInsufficientBalance_ReturnsFalse()
    // {
    //     // Arrange
    //     const int sourceLedgerId = 1;
    //     const int destinationLedgerId = 2;
    //     const decimal transferAmount = 150m;
    //
    //     _context.Ledgers.AddRange(
    //         new Ledger { Id = sourceLedgerId, Balance = 100m },
    //         new Ledger { Id = destinationLedgerId, Balance = 100m }
    //     );
    //     await _context.SaveChangesAsync();
    //
    //     var repository = CreateTestee();
    //
    //     // Act
    //     var result = await repository.Book(sourceLedgerId, destinationLedgerId, transferAmount, CancellationToken.None);
    //
    //     // Assert
    //     Assert.False(result);
    // }
}