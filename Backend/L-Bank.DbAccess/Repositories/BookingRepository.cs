﻿using System.Data.Common;
using L_Bank_W_Backend.DbAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace L_Bank_W_Backend.DbAccess.Repositories;

public class BookingRepository(AppDbContext context, ILogger<BookingRepository> logger) : IBookingRepository
{
    public async Task<bool> Book(int sourceLedgerId, int destinationLedgerId, decimal amount, CancellationToken ct)
    {
        var sql = @"
        BEGIN TRANSACTION;

        DECLARE @SourceBalance DECIMAL(19,4);
        DECLARE @DestinationBalance DECIMAL(19,4);

        SELECT @SourceBalance = Balance FROM Ledgers WHERE Id = @sourceId;
        SELECT @DestinationBalance = Balance FROM Ledgers WHERE Id = @destinationId;

        IF @SourceBalance >= @Amount
        BEGIN
            UPDATE Ledgers SET Balance = Balance - @Amount WHERE Id = @sourceId;
            UPDATE Ledgers SET Balance = Balance + @Amount WHERE Id = @destinationId;

            COMMIT TRANSACTION;
            SELECT CAST(1 AS BIT); -- True for success
        END
        ELSE
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT CAST(0 AS BIT); -- False for insufficient funds
        END";

        var parameters = new[]
        {
            context.Database.GetDbConnection().CreateParameter("@sourceId", sourceLedgerId),
            context.Database.GetDbConnection().CreateParameter("@destinationId", destinationLedgerId),
            context.Database.GetDbConnection().CreateParameter("@Amount", amount)
        };

        try
        {
            var result = await context.Database.ExecuteSqlRawAsync(sql, parameters, ct);
            return result == 2;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Booking transaction failed");
            throw;
        }
    }
}

public static class DbConnectionExtensions
{
    public static DbParameter CreateParameter(this DbConnection conn, string name, object value)
    {
        var parameter = conn.CreateCommand().CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        return parameter;
    }
}