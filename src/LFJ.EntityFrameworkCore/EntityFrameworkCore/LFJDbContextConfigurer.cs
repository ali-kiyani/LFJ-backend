using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace LFJ.EntityFrameworkCore
{
    public static class LFJDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<LFJDbContext> builder, string connectionString)
        {
            builder.UseSqlServer(connectionString);
        }

        public static void Configure(DbContextOptionsBuilder<LFJDbContext> builder, DbConnection connection)
        {
            builder.UseSqlServer(connection);
        }
    }
}
