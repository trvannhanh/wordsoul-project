using FluentAssertions;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;
using WordSoul.Infrastructure.Persistence.Repositories;
using WordSoul.IntegrationTests.Fixtures;

namespace WordSoul.IntegrationTests.Repositories;

public class AnswerRecordRepositoryTests : IntegrationTestBase
{
    [Fact]
    public async Task GetAttemptCountAsync_ReturnsHighestPersistedAttemptNumber()
    {
        var user = await _dataBuilder.CreateUserAsync("attempt_user");
        var vocabulary = await _dataBuilder.CreateVocabularyAsync("atomic", "nguyên tử");
        var session = await _dataBuilder.CreateReviewSessionAsync(
            user.Id,
            [vocabulary]);

        _context.AnswerRecords.AddRange(
            CreateAnswer(session.Id, vocabulary.Id, 1),
            CreateAnswer(session.Id, vocabulary.Id, 3),
            CreateAnswer(session.Id, vocabulary.Id, 2));
        await _context.SaveChangesAsync();

        var repository = new AnswerRecordRepository(_context);
        var attemptNumber = await repository.GetAttemptCountAsync(
            session.Id,
            vocabulary.Id,
            QuestionType.Flashcard);

        attemptNumber.Should().Be(3);
    }

    [Fact]
    public async Task GetAttemptCountAsync_NoAttempts_ReturnsZero()
    {
        var repository = new AnswerRecordRepository(_context);

        var attemptNumber = await repository.GetAttemptCountAsync(
            999,
            999,
            QuestionType.Flashcard);

        attemptNumber.Should().Be(0);
    }

    private static AnswerRecord CreateAnswer(
        int sessionId,
        int vocabularyId,
        int attemptNumber)
    {
        return new AnswerRecord
        {
            SubmissionId = Guid.NewGuid(),
            LearningSessionId = sessionId,
            VocabularyId = vocabularyId,
            QuestionType = QuestionType.Flashcard,
            Answer = "viewed",
            IsCorrect = true,
            AttemptCount = attemptNumber,
            ResultingLevel = 1,
            CreatedAt = DateTime.UtcNow.AddSeconds(attemptNumber)
        };
    }
}
