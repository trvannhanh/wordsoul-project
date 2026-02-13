

using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using WordSoul.Application.Services.SRS;
using WordSoul.IntegrationTests.Fakes;

namespace WordSoul.IntegrationTests.Services.SRS
{
    /// <summary>
    /// Integration tests for SRSService
    /// Tests real database interactions and SRS algorithm integration
    /// </summary>
    public class SRSServiceIntegrationTests : IntegrationTestBase
    {
        private readonly SRSService _srsService;
        private readonly FakeTimeProvider _timeProvider;

        public SRSServiceIntegrationTests()
        {

            _timeProvider = new FakeTimeProvider
            {
                UtcNow = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            // Create service with real dependencies
            _srsService = new SRSService(
                _unitOfWork,
                _srsAlgorithm,
                NullLogger<SRSService>.Instance,
                _timeProvider
            );
        }

        #region UpdateAfterReviewAsync Tests

        [Fact]
        public async Task UpdateAfterReviewAsync_WithGrade5_ShouldIncreaseEFAndInterval()
        {
            // ARRANGE
            var user = await _dataBuilder.CreateUserAsync("testuser1");
            var vocab = await _dataBuilder.CreateVocabularyAsync("perseverance", "sự kiên trì");
            var progress = await _dataBuilder.CreateProgressAsync(
                user.Id,
                vocab.Id,
                easeFactor: 2.5,
                interval: 6,
                repetition: 2
            );

            int grade = 5;  // Perfect recall

            // ACT
            var result = await _srsService.UpdateAfterReviewAsync(
                user.Id,
                vocab.Id,
                grade
            );

            // ASSERT
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.NewEaseFactor.Should().BeGreaterThan(2.5);
            result.NewInterval.Should().BeGreaterThan(6);
            result.MemoryState.Should().Be("Review");
            result.Message.Should().Contain("Perfect");

            // Verify database updated
            var updatedProgress = await _unitOfWork.UserVocabularyProgress
                .GetUserVocabularyProgressAsync(user.Id, vocab.Id);

            updatedProgress.Should().NotBeNull();
            updatedProgress!.EasinessFactor.Should().BeApproximately(2.6, 0.01);
            updatedProgress.Repetition.Should().Be(3);
            updatedProgress.LastGrade.Should().Be(5);
            updatedProgress.RetentionScore.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task UpdateAfterReviewAsync_WithGrade0_ShouldResetProgress()
        {
            // ARRANGE
            var user = await _dataBuilder.CreateUserAsync("testuser2");
            var vocab = await _dataBuilder.CreateVocabularyAsync("oblivion", "sự quên lãng");
            var progress = await _dataBuilder.CreateProgressAsync(
                user.Id,
                vocab.Id,
                easeFactor: 2.5,
                interval: 15,
                repetition: 5  // Was doing well
            );

            int grade = 0;  // Complete forget

            // ACT
            var result = await _srsService.UpdateAfterReviewAsync(
                user.Id,
                vocab.Id,
                grade
            );

            // ASSERT
            result.NewEaseFactor.Should().BeLessThan(2.5);
            result.NewInterval.Should().Be(0);  // Review now!
            result.MemoryState.Should().Be("Relearning");

            // Verify database
            var updatedProgress = await _unitOfWork.UserVocabularyProgress
                .GetUserVocabularyProgressAsync(user.Id, vocab.Id);

            updatedProgress!.Repetition.Should().Be(0);  // RESET!
            updatedProgress.LastGrade.Should().Be(0);
        }

        //[Fact]
        //public async Task UpdateAfterReviewAsync_ShouldSaveReviewHistory()
        //{
        //    // ARRANGE
        //    var user = await _dataBuilder.CreateUserAsync("testuser3");
        //    var vocab = await _dataBuilder.CreateVocabularyAsync("ephemeral", "tạm thời");
        //    await _dataBuilder.CreateProgressAsync(user.Id, vocab.Id);

        //    // ACT
        //    await _srsService.UpdateAfterReviewAsync(user.Id, vocab.Id, grade: 4);

        //    // ASSERT
        //    var history = await _unitOfWork.VocabularyReviewHistory
        //        .GetReviewHistoryByVocabularyAsync(vocab.Id);

        //    history.Should().NotBeEmpty();
        //    var lastReview = history.First();
        //    lastReview.Grade.Should().Be(4);
        //    lastReview.IsCorrect.Should().BeTrue();
        //}

        [Fact]
        public async Task UpdateAfterReviewAsync_WithNonExistentProgress_ShouldThrowException()
        {
            // ARRANGE
            var user = await _dataBuilder.CreateUserAsync("testuser4");
            var vocab = await _dataBuilder.CreateVocabularyAsync("test", "thử nghiệm");
            // Note: NOT creating progress!

            // ACT
            Func<Task> act = async () => await _srsService.UpdateAfterReviewAsync(
                user.Id,
                vocab.Id,
                grade: 3
            );

            // ASSERT
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("*Progress not found*");
        }

        [Theory]
        [InlineData(3, "Learning")]  // Rep 0→1
        [InlineData(4, "Learning")]  // Rep 1→2
        [InlineData(5, "Review")]    // Rep 2→3
        public async Task UpdateAfterReviewAsync_ShouldTransitionMemoryStates(
            int grade,
            string expectedState)
        {
            // ARRANGE
            var user = await _dataBuilder.CreateUserAsync($"testuser_{grade}");
            var vocab = await _dataBuilder.CreateVocabularyAsync($"word_{grade}", "meaning");
            await _dataBuilder.CreateProgressAsync(
                user.Id,
                vocab.Id,
                repetition: grade == 5 ? 2 : (grade == 4 ? 1 : 0)
            );

            // ACT
            var result = await _srsService.UpdateAfterReviewAsync(
                user.Id,
                vocab.Id,
                grade
            );

            // ASSERT
            result.MemoryState.Should().Be(expectedState);
        }

        #endregion

        #region GetDueVocabulariesAsync Tests

        [Fact]
        public async Task GetDueVocabulariesAsync_ShouldReturnOnlyDueWords()
        {
            // ARRANGE
            var user = await _dataBuilder.CreateUserAsync("testuser5");

            // Từ đã quá hạn
            var vocab1 = await _dataBuilder.CreateVocabularyAsync("overdue", "quá hạn");
            await _dataBuilder.CreateProgressAsync(
                user.Id,
                vocab1.Id,
                interval: 0
            );
            _context.UserVocabularyProgresses
                    .First(p => p.VocabularyId == vocab1.Id)
                    .NextReviewTime = _timeProvider.UtcNow.AddDays(-1); 
            await _context.SaveChangesAsync();

            // Từ due hôm nay
            var vocab2 = await _dataBuilder.CreateVocabularyAsync("today", "hôm nay");
            await _dataBuilder.CreateProgressAsync(
                user.Id,
                vocab2.Id,
                interval: 0
            );

            _context.UserVocabularyProgresses
                .First(p => p.VocabularyId == vocab2.Id)
                .NextReviewTime = _timeProvider.UtcNow;

            await _context.SaveChangesAsync();

            // Từ chưa due
            var vocab3 = await _dataBuilder.CreateVocabularyAsync("future", "tương lai");
            await _dataBuilder.CreateProgressAsync(
                user.Id,
                vocab3.Id,
                interval: 10
            );

            // ACT
            var dueWords = await _srsService.GetDueVocabulariesAsync(user.Id, limit: 10);

            // ASSERT
            dueWords.Should().HaveCount(2);
            dueWords.Should().Contain(w => w.Word == "overdue");
            dueWords.Should().Contain(w => w.Word == "today");
            dueWords.Should().NotContain(w => w.Word == "future");
        }

        [Fact]
        public async Task GetDueVocabulariesAsync_ShouldPrioritizeOverdueWords()
        {
            // ARRANGE
            var user = await _dataBuilder.CreateUserAsync("testuser6");

            var vocab1 = await _dataBuilder.CreateVocabularyAsync("word1", "từ 1");
            await _dataBuilder.CreateProgressAsync(user.Id, vocab1.Id);
            _context.UserVocabularyProgresses
                    .First(p => p.VocabularyId == vocab1.Id)
                    .NextReviewTime = _timeProvider.UtcNow.AddDays(-5);

            var vocab2 = await _dataBuilder.CreateVocabularyAsync("word2", "từ 2");
            await _dataBuilder.CreateProgressAsync(user.Id, vocab2.Id);
            _context.UserVocabularyProgresses
                    .First(p => p.VocabularyId == vocab2.Id)
                    .NextReviewTime = _timeProvider.UtcNow.AddDays(-1);

            await _context.SaveChangesAsync();

            // ACT
            var dueWords = await _srsService.GetDueVocabulariesAsync(user.Id);

            // ASSERT
            dueWords.Should().NotBeEmpty();
            dueWords.First().Word.Should().Be("word1");  // 5 days overdue first
            dueWords.First().DaysOverdue.Should().Be(5);
        }

        #endregion

        #region GetOverallRetentionScoreAsync Tests

        [Fact]
        public async Task GetOverallRetentionScoreAsync_ShouldCalculateAverageScore()
        {
            // ARRANGE
            var user = await _dataBuilder.CreateUserAsync("testuser7");

            // Word 1: 100% accuracy
            var vocab1 = await _dataBuilder.CreateVocabularyAsync("word1", "từ 1");
            await _dataBuilder.CreateProgressAsync(user.Id, vocab1.Id);
            _context.UserVocabularyProgresses.First(p => p.VocabularyId == vocab1.Id)
                .CorrectAttempt = 10;
            _context.UserVocabularyProgresses.First(p => p.VocabularyId == vocab1.Id)
                .TotalAttempt = 10;

            // Word 2: 50% accuracy
            var vocab2 = await _dataBuilder.CreateVocabularyAsync("word2", "từ 2");
            await _dataBuilder.CreateProgressAsync(user.Id, vocab2.Id);
            _context.UserVocabularyProgresses.First(p => p.VocabularyId == vocab2.Id)
                .CorrectAttempt = 5;
            _context.UserVocabularyProgresses.First(p => p.VocabularyId == vocab2.Id)
                .TotalAttempt = 10;

            await _context.SaveChangesAsync();

            // ACT
            var overallScore = await _srsService.GetOverallRetentionScoreAsync(user.Id);

            // ASSERT
            // (100 + 50) / 2 = 75
            overallScore.Should().Be(75);
        }

        #endregion
    }

}
