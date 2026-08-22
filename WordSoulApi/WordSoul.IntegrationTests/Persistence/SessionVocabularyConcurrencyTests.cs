using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;
using WordSoul.Infrastructure.Persistence;

namespace WordSoul.IntegrationTests.Persistence;

public class SessionVocabularyConcurrencyTests
{
    [Fact]
    public async Task ConcurrentStageUpdates_SecondWriterIsRejected()
    {
        var databaseName = $"wordsoul-concurrency-{Guid.NewGuid():N}";
        var connectionString = $"Data Source={databaseName};Mode=Memory;Cache=Shared";

        await using var anchor = new SqliteConnection(connectionString);
        await anchor.OpenAsync();

        var options = new DbContextOptionsBuilder<WordSoulDbContext>()
            .UseSqlite(connectionString)
            .Options;

        await using var firstContext = new WordSoulDbContext(options);
        await firstContext.Database.EnsureCreatedAsync();

        var user = new User
        {
            Username = "concurrency_user",
            Email = "concurrency@test.com",
            PasswordHash = "hash",
            Role = UserRole.User,
            IsActive = true
        };
        var vocabulary = new Vocabulary
        {
            Word = "atomic",
            Meaning = "indivisible",
            PartOfSpeech = PartOfSpeech.Adjective,
            CEFRLevel = CEFRLevel.B2
        };
        var session = new LearningSession
        {
            User = user,
            Type = SessionType.Learning,
            SessionVocabularies =
            [
                new SessionVocabulary
                {
                    Vocabulary = vocabulary,
                    CurrentStageIndex = 0,
                    Order = 1
                }
            ]
        };

        firstContext.Add(session);
        await firstContext.SaveChangesAsync();
        firstContext.ChangeTracker.Clear();

        await using var secondContext = new WordSoulDbContext(options);
        var firstWriter = await firstContext.SessionVocabularies.SingleAsync();
        var secondWriter = await secondContext.SessionVocabularies.SingleAsync();

        firstWriter.CurrentStageIndex = 1;
        firstWriter.ConcurrencyToken = Guid.NewGuid();
        secondWriter.CurrentStageIndex = 1;
        secondWriter.ConcurrencyToken = Guid.NewGuid();

        await firstContext.SaveChangesAsync();
        var secondSave = () => secondContext.SaveChangesAsync();

        await secondSave.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }
}
