using Microsoft.EntityFrameworkCore;
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
            var options = new DbContextOptionsBuilder<WordSoulDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new WordSoulDbContext(options);
            context.Database.EnsureCreated();

            return context;
        }
    }
}
