﻿using L_Bank_W_Backend.DbAccess;
using L_Bank_W_Backend.DbAccess.Data;
using L_Bank_W_Backend.DbAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace L_Bank.Test.Concurrent;

public class UnitTest1
{
    private readonly Mock<IOptions<DatabaseSettings>> _databaseSettingsMock;
    private readonly Mock<ILogger<LedgerRepository>> _loggerMock;

    public UnitTest1()
    {
        _databaseSettingsMock = new Mock<IOptions<DatabaseSettings>>();
        _databaseSettingsMock.Setup(x => x.Value).Returns(new DatabaseSettings
        {
            ConnectionString =
                "Server=localhost,1433;Database=l_bank_backend;User Id=sa;Password=YourPassword123;TrustServerCertificate=True;"
        });

        _loggerMock = new Mock<ILogger<LedgerRepository>>();
    }

    [Fact]
    public async Task Test_Service_Using_RealSqlServer()
    {
        var connectionString =
            "Server=localhost,1433;Database=l_bank_backend;User Id=sa;Password=YourPassword123;TrustServerCertificate=True;";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using (var context = new AppDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();
        }

        await using (var context = new AppDbContext(options))
        {
            var service = new LedgerRepository(_databaseSettingsMock.Object, context, _loggerMock.Object);
            var result = await service.GetAllLedgers(new CancellationToken());
            Assert.NotNull(result);
        }
    }
}