using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace HodHod.EntityFrameworkCore;

public static class HodHodDbContextConfigurer
{
    public static void Configure(DbContextOptionsBuilder<HodHodDbContext> builder, string connectionString)
    {
        builder.UseSqlServer(connectionString);
    }

    public static void Configure(DbContextOptionsBuilder<HodHodDbContext> builder, DbConnection connection)
    {
        builder.UseSqlServer(connection);
    }
}

