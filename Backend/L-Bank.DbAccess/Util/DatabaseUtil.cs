using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace L_Bank_W_Backend.DbAccess.Util;

public static class DatabaseUtil
{
    public static int ComputeExponentialBackoff(int retries)
    {
        return (int)(Math.Pow(2, retries) * 100);
    }

    public static bool IsDeadlock(DbUpdateConcurrencyException ex)
    {
        var innerException = ex.InnerException as SqlException;
        return innerException != null && innerException.Number == 1205;
    }
}