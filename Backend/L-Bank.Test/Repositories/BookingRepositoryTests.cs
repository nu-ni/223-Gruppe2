using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.DbAccess.Data;
using L_Bank_W_Backend.DbAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

public class BookingRepositoryTests
{
    private readonly Mock<DbSet<Ledger>> _mockLedgerSet = new Mock<DbSet<Ledger>>();
    private readonly Mock<AppDbContext> _mockContext = new Mock<AppDbContext>();
    private readonly Mock<ILogger<BookingRepository>> _loggerMock = new();

    public BookingRepositoryTests()
    {
        // Initialize your mock data
        var ledgers = new List<Ledger>
        {
            new Ledger { Id = 1, Balance = 100m },
            new Ledger { Id = 2, Balance = 100m }
        }.AsQueryable();

        // Setup the mock set
        _mockLedgerSet.As<IQueryable<Ledger>>().Setup(m => m.Provider).Returns(ledgers.Provider);
        _mockLedgerSet.As<IQueryable<Ledger>>().Setup(m => m.Expression).Returns(ledgers.Expression);
        _mockLedgerSet.As<IQueryable<Ledger>>().Setup(m => m.ElementType).Returns(ledgers.ElementType);
        _mockLedgerSet.As<IQueryable<Ledger>>().Setup(m => m.GetEnumerator()).Returns(ledgers.GetEnumerator());

        // Setup the mock context
        _mockContext.Setup(c => c.Ledgers).Returns(_mockLedgerSet.Object);

        // Configure the context to increment balances as a result of the Book method
        _mockContext.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                var sourceLedger = ledgers.Single(l => l.Id == 1);
                var destinationLedger = ledgers.Single(l => l.Id == 2);
                sourceLedger.Balance -= 50;
                destinationLedger.Balance += 50;
            })
            .ReturnsAsync(1); // Return a task result of 1 change tracked
    }

    [Fact]
    public async Task Book_WithValidLedgers_TransfersFundsAndReturnsTrue()
    {
        // Set up your test with the mocked context.
        var repo = new BookingRepository(_mockContext.Object, _loggerMock.Object);

        // Act
        var success = await repo.Book(1, 2, 50m, new CancellationToken());

        // Assert
        Assert.True(success);
        _mockLedgerSet.Verify(m => m.Update(It.IsAny<Ledger>()), Times.Exactly(2));
        _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }
}