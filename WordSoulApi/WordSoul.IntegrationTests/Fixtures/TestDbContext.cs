using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using WordSoul.Infrastructure.Persistence;

namespace WordSoul.IntegrationTests.Fixtures
{
    /// <summary>
    /// In-memory database context for integration testing
    /// </summary>
    public class TestDbContextFactory
    {
        public static WordSoulDbContext Create()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<WordSoulDbContext>()
                .UseSqlite(connection, contextOwnsConnection: true)
                .Options;

            var context = new WordSoulDbContext(options);
            context.Database.EnsureCreated();

            return context;
        }
    }
}
