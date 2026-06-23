using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WordSoul.Application.Common;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Application.Services.SRS;
using WordSoul.Domain.Entities;

namespace WordSoul.Tests.Services.SRS
{
    /// <summary>
    /// Unit tests cho SRSService — orchestration layer (mock IUnitOfWork, real SRSAlgorithm)
    /// Covers: UpdateAfterReviewAsync, GetDueVocabulariesAsync, GetOverallRetentionScoreAsync
    /// </summary>
    public class SRSServiceTests
    {
        // Fixed "now" cho tất cả tests — đảm bảo deterministic
        private static readonly DateTime FakeNow =
            new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);

        // ─── Lightweight ITimeProvider fake ───────────────────────────────
        private sealed class FixedTime : ITimeProvider
        {
            private readonly DateTime _now;
            public FixedTime(DateTime now) => _now = now;
            public DateTime UtcNow => _now;
        }

        // ─── Factory helper ────────────────────────────────────────────────
        private static (
            SRSService service,
            Mock<IUnitOfWork> uowMock,
            Mock<IUserVocabularyProgressRepository> progressRepoMock)
            CreateService(DateTime? now = null)
        {
            var uowMock = new Mock<IUnitOfWork>();
            var progressRepoMock = new Mock<IUserVocabularyProgressRepository>();

            uowMock.SetupGet(x => x.UserVocabularyProgress).Returns(progressRepoMock.Object);
            uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(1);

            // Default: UpdateSrsParametersAsync echoes the entity back
            progressRepoMock
                .Setup(r => r.UpdateSrsParametersAsync(
                    It.IsAny<UserVocabularyProgress>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserVocabularyProgress p, CancellationToken _) => p);

            var algorithm = new SRSAlgorithm();
            var logger = new Mock<ILogger<SRSService>>();
            var timeProvider = new FixedTime(now ?? FakeNow);

            var service = new SRSService(
                uowMock.Object, algorithm, logger.Object, timeProvider);

            return (service, uowMock, progressRepoMock);
        }

        // ─── Progress entity builder ───────────────────────────────────────
        private static UserVocabularyProgress MakeProgress(
            int userId = 1,
            int vocabId = 1,
            double ef = 2.5,
            int interval = 6,
            int rep = 2,
            int correctAttempt = 8,
            int totalAttempt = 10,
            DateTime? firstLearnedAt = null,
            DateTime? masteredAt = null)
        {
            return new UserVocabularyProgress
            {
                UserId = userId,
                VocabularyId = vocabId,
                EasinessFactor = ef,
                Interval = interval,
                Repetition = rep,
                NextReviewTime = FakeNow.AddDays(-1),
                CorrectAttempt = correctAttempt,
                TotalAttempt = totalAttempt,
                FirstLearnedAt = firstLearnedAt,
                MasteredAt = masteredAt,
                MemoryState = "Review"
            };
        }

        // ──────────────────────────────────────────────────────────────────
        #region UpdateAfterReviewAsync
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAfterReviewAsync_ProgressNotFound_ThrowsKeyNotFoundException()
        {
            // ARRANGE
            var (service, _, progressRepoMock) = CreateService();
            progressRepoMock
                .Setup(r => r.GetUserVocabularyProgressAsync(1, 99, default))
                .ReturnsAsync((UserVocabularyProgress?)null);

            // ACT
            Func<Task> act = () => service.UpdateAfterReviewAsync(1, 99, grade: 4);

            // ASSERT
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("*Progress not found*");
        }

        [Fact]
        public async Task UpdateAfterReviewAsync_Grade5_IncreasesEFAndIntervalReturnsSuccess()
        {
            // ARRANGE  — rep=2, interval=6, ef=2.5, grade=5
            // SM-2: EF' = 2.5 + (0.1 - 0*...) = 2.5 + 0.1 = 2.6
            // NewRepetition=3, NewInterval = ceil(6 * 2.6) = 16
            var (service, _, progressRepoMock) = CreateService();
            var progress = MakeProgress(ef: 2.5, interval: 6, rep: 2);
            progressRepoMock
                .Setup(r => r.GetUserVocabularyProgressAsync(1, 1, default))
                .ReturnsAsync(progress);

            // ACT
            var result = await service.UpdateAfterReviewAsync(1, 1, grade: 5);

            // ASSERT
            result.Success.Should().BeTrue();
            result.NewEaseFactor.Should().BeGreaterThan(2.5);
            result.NewInterval.Should().BeGreaterThan(6);
            result.MemoryState.Should().Be("Review");
            result.Message.Should().Contain("Perfect");
        }

        [Fact]
        public async Task UpdateAfterReviewAsync_Grade0_ResetsRepetitionAndDecreasesEF()
        {
            // ARRANGE — grade=0 fails: rep resets to 0, interval=0, EF drops
            var (service, _, progressRepoMock) = CreateService();
            var progress = MakeProgress(ef: 2.5, interval: 6, rep: 3);
            progressRepoMock
                .Setup(r => r.GetUserVocabularyProgressAsync(1, 1, default))
                .ReturnsAsync(progress);

            // ACT
            var result = await service.UpdateAfterReviewAsync(1, 1, grade: 0);

            // ASSERT
            result.NewEaseFactor.Should().BeLessThan(2.5);
            result.NewInterval.Should().Be(0);
            result.Message.Should().Contain("worry");
        }

        [Fact]
        public async Task UpdateAfterReviewAsync_FirstLearnedAtNull_SetToNow()
        {
            // ARRANGE — FirstLearnedAt not yet set
            var (service, _, progressRepoMock) = CreateService();
            var progress = MakeProgress(firstLearnedAt: null);

            UserVocabularyProgress? captured = null;
            progressRepoMock
                .Setup(r => r.GetUserVocabularyProgressAsync(1, 1, default))
                .ReturnsAsync(progress);
            progressRepoMock
                .Setup(r => r.UpdateSrsParametersAsync(
                    It.IsAny<UserVocabularyProgress>(), It.IsAny<CancellationToken>()))
                .Callback<UserVocabularyProgress, CancellationToken>((p, _) => captured = p)
                .ReturnsAsync((UserVocabularyProgress p, CancellationToken _) => p);

            // ACT
            await service.UpdateAfterReviewAsync(1, 1, grade: 4);

            // ASSERT
            captured!.FirstLearnedAt.Should().Be(FakeNow);
        }

        [Fact]
        public async Task UpdateAfterReviewAsync_FirstLearnedAtAlreadySet_DoesNotChange()
        {
            // ARRANGE
            var alreadySet = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);
            var (service, _, progressRepoMock) = CreateService();
            var progress = MakeProgress(firstLearnedAt: alreadySet);

            UserVocabularyProgress? captured = null;
            progressRepoMock
                .Setup(r => r.GetUserVocabularyProgressAsync(1, 1, default))
                .ReturnsAsync(progress);
            progressRepoMock
                .Setup(r => r.UpdateSrsParametersAsync(
                    It.IsAny<UserVocabularyProgress>(), It.IsAny<CancellationToken>()))
                .Callback<UserVocabularyProgress, CancellationToken>((p, _) => captured = p)
                .ReturnsAsync((UserVocabularyProgress p, CancellationToken _) => p);

            // ACT
            await service.UpdateAfterReviewAsync(1, 1, grade: 4);

            // ASSERT
            captured!.FirstLearnedAt.Should().Be(alreadySet);
        }

        [Fact]
        public async Task UpdateAfterReviewAsync_MasteredTransition_SetsMasteredAt()
        {
            // ARRANGE — rep=6, interval=9, EF=2.5, grade=4
            // NewEF = 2.5 (grade4 → no change)
            // NewRep = 7
            // NewInterval = ceil(9 * 2.5) = ceil(22.5) = 23  ≥ 21  → "Mastered"
            var (service, _, progressRepoMock) = CreateService();
            var progress = MakeProgress(ef: 2.5, interval: 9, rep: 6, masteredAt: null);

            UserVocabularyProgress? captured = null;
            progressRepoMock
                .Setup(r => r.GetUserVocabularyProgressAsync(1, 1, default))
                .ReturnsAsync(progress);
            progressRepoMock
                .Setup(r => r.UpdateSrsParametersAsync(
                    It.IsAny<UserVocabularyProgress>(), It.IsAny<CancellationToken>()))
                .Callback<UserVocabularyProgress, CancellationToken>((p, _) => captured = p)
                .ReturnsAsync((UserVocabularyProgress p, CancellationToken _) => p);

            // ACT
            var result = await service.UpdateAfterReviewAsync(1, 1, grade: 4);

            // ASSERT
            result.MemoryState.Should().Be("Mastered");
            captured!.MasteredAt.Should().Be(FakeNow);
        }

        [Fact]
        public async Task UpdateAfterReviewAsync_AlreadyMastered_DoesNotOverwriteMasteredAt()
        {
            // ARRANGE — same calc, but MasteredAt already stamped
            var originalMasteredAt = new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc);
            var (service, _, progressRepoMock) = CreateService();
            var progress = MakeProgress(ef: 2.5, interval: 9, rep: 6, masteredAt: originalMasteredAt);

            UserVocabularyProgress? captured = null;
            progressRepoMock
                .Setup(r => r.GetUserVocabularyProgressAsync(1, 1, default))
                .ReturnsAsync(progress);
            progressRepoMock
                .Setup(r => r.UpdateSrsParametersAsync(
                    It.IsAny<UserVocabularyProgress>(), It.IsAny<CancellationToken>()))
                .Callback<UserVocabularyProgress, CancellationToken>((p, _) => captured = p)
                .ReturnsAsync((UserVocabularyProgress p, CancellationToken _) => p);

            // ACT
            await service.UpdateAfterReviewAsync(1, 1, grade: 4);

            // ASSERT — MasteredAt should NOT be overwritten
            captured!.MasteredAt.Should().Be(originalMasteredAt);
        }

        [Fact]
        public async Task UpdateAfterReviewAsync_SaveChangesAsync_NeverCalled()
        {
            // ARRANGE
            var (service, uowMock, progressRepoMock) = CreateService();
            var progress = MakeProgress();
            progressRepoMock
                .Setup(r => r.GetUserVocabularyProgressAsync(1, 1, default))
                .ReturnsAsync(progress);

            // ACT
            await service.UpdateAfterReviewAsync(1, 1, grade: 3);

            // ASSERT
            uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAfterReviewAsync_OldValuesPreservedInResult()
        {
            // ARRANGE
            var (service, _, progressRepoMock) = CreateService();
            var progress = MakeProgress(ef: 2.5, interval: 6, rep: 2);
            var oldNextReview = progress.NextReviewTime;

            progressRepoMock
                .Setup(r => r.GetUserVocabularyProgressAsync(1, 1, default))
                .ReturnsAsync(progress);

            // ACT
            var result = await service.UpdateAfterReviewAsync(1, 1, grade: 5);

            // ASSERT — OldXxx must reflect pre-update values
            result.OldEaseFactor.Should().Be(2.5);
            result.OldInterval.Should().Be(6);
            result.OldRepetition.Should().Be(2);
            result.OldNextReviewDate.Should().Be(oldNextReview);
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region GetDueVocabulariesAsync
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetDueVocabulariesAsync_EmptyRepo_ReturnsEmpty()
        {
            // ARRANGE
            var (service, _, progressRepoMock) = CreateService();
            progressRepoMock
                .Setup(r => r.GetDueVocabulariesAsync(1, It.IsAny<DateTime>(), default))
                .ReturnsAsync(new List<UserVocabularyProgress>());

            // ACT
            var result = await service.GetDueVocabulariesAsync(1);

            // ASSERT
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDueVocabulariesAsync_ReturnsMappedDto()
        {
            // ARRANGE
            var (service, _, progressRepoMock) = CreateService();
            var dueTime = FakeNow.AddDays(-2);
            var progresses = new List<UserVocabularyProgress>
            {
                new()
                {
                    UserId = 1, VocabularyId = 5,
                    NextReviewTime = dueTime,
                    Repetition = 3,
                    CorrectAttempt = 8, TotalAttempt = 10,
                    Vocabulary = new Vocabulary
                        { Id = 5, Word = "ephemeral", Meaning = "tạm thời" }
                }
            };
            progressRepoMock
                .Setup(r => r.GetDueVocabulariesAsync(1, FakeNow, default))
                .ReturnsAsync(progresses);

            // ACT
            var result = await service.GetDueVocabulariesAsync(1);

            // ASSERT
            result.Should().HaveCount(1);
            result[0].VocabularyId.Should().Be(5);
            result[0].Word.Should().Be("ephemeral");
            result[0].NextReviewDate.Should().Be(dueTime);
            result[0].Repetition.Should().Be(3);
        }

        [Fact]
        public async Task GetDueVocabulariesAsync_OrderedByNextReviewTimeAscending()
        {
            // ARRANGE — 3 overdue words with different NextReviewTime
            var (service, _, progressRepoMock) = CreateService();
            var progresses = new List<UserVocabularyProgress>
            {
                new() { UserId=1, VocabularyId=1, NextReviewTime=FakeNow.AddDays(-1),
                        Vocabulary=new Vocabulary { Id=1, Word="word1" } },
                new() { UserId=1, VocabularyId=2, NextReviewTime=FakeNow.AddDays(-5),
                        Vocabulary=new Vocabulary { Id=2, Word="word2" } },
                new() { UserId=1, VocabularyId=3, NextReviewTime=FakeNow.AddDays(-3),
                        Vocabulary=new Vocabulary { Id=3, Word="word3" } },
            };
            progressRepoMock
                .Setup(r => r.GetDueVocabulariesAsync(1, FakeNow, default))
                .ReturnsAsync(progresses);

            // ACT
            var result = await service.GetDueVocabulariesAsync(1, limit: 10);

            // ASSERT — oldest-overdue first
            result[0].Word.Should().Be("word2");  // -5 days
            result[1].Word.Should().Be("word3");  // -3 days
            result[2].Word.Should().Be("word1");  // -1 day
        }

        [Fact]
        public async Task GetDueVocabulariesAsync_RespectsLimit()
        {
            // ARRANGE — 10 words, ask for limit=3
            var (service, _, progressRepoMock) = CreateService();
            var progresses = Enumerable.Range(1, 10)
                .Select(i => new UserVocabularyProgress
                {
                    UserId = 1, VocabularyId = i,
                    NextReviewTime = FakeNow.AddDays(-i),
                    Vocabulary = new Vocabulary { Id = i, Word = $"word{i}" }
                })
                .ToList();
            progressRepoMock
                .Setup(r => r.GetDueVocabulariesAsync(1, FakeNow, default))
                .ReturnsAsync(progresses);

            // ACT
            var result = await service.GetDueVocabulariesAsync(1, limit: 3);

            // ASSERT
            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetDueVocabulariesAsync_DaysOverdue_CalculatedCorrectly()
        {
            // ARRANGE — word overdue by exactly 7 days
            var (service, _, progressRepoMock) = CreateService();
            var progress = new UserVocabularyProgress
            {
                UserId = 1, VocabularyId = 1,
                NextReviewTime = FakeNow.AddDays(-7),
                Vocabulary = new Vocabulary { Id = 1, Word = "overdue" }
            };
            progressRepoMock
                .Setup(r => r.GetDueVocabulariesAsync(1, FakeNow, default))
                .ReturnsAsync(new List<UserVocabularyProgress> { progress });

            // ACT
            var result = await service.GetDueVocabulariesAsync(1);

            // ASSERT
            result[0].DaysOverdue.Should().Be(7);
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region GetOverallRetentionScoreAsync
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetOverallRetentionScoreAsync_NoProgresses_ReturnsZero()
        {
            // ARRANGE
            var (service, _, progressRepoMock) = CreateService();
            progressRepoMock
                .Setup(r => r.GetAllUserVocabularyProgressByUserAsync(1, default))
                .ReturnsAsync(new List<UserVocabularyProgress>());

            // ACT
            var score = await service.GetOverallRetentionScoreAsync(1);

            // ASSERT
            score.Should().Be(0);
        }

        [Fact]
        public async Task GetOverallRetentionScoreAsync_SingleProgress_ReturnsCorrectScore()
        {
            // ARRANGE — 10 correct / 10 total, rep=0  →  100% accuracy + 0 bonus = 100
            var (service, _, progressRepoMock) = CreateService();
            var progress = new UserVocabularyProgress
            {
                UserId = 1, VocabularyId = 1,
                CorrectAttempt = 10, TotalAttempt = 10, Repetition = 0
            };
            progressRepoMock
                .Setup(r => r.GetAllUserVocabularyProgressByUserAsync(1, default))
                .ReturnsAsync(new List<UserVocabularyProgress> { progress });

            // ACT
            var score = await service.GetOverallRetentionScoreAsync(1);

            // ASSERT
            score.Should().Be(100);
        }

        [Fact]
        public async Task GetOverallRetentionScoreAsync_MultipleProgresses_ReturnsAverage()
        {
            // ARRANGE
            // Word1: 10/10 correct, rep=0  → score = 100
            // Word2: 5/10 correct,  rep=0  → score = 50
            // Average = 75
            var (service, _, progressRepoMock) = CreateService();
            var progresses = new List<UserVocabularyProgress>
            {
                new() { UserId=1, VocabularyId=1, CorrectAttempt=10, TotalAttempt=10, Repetition=0 },
                new() { UserId=1, VocabularyId=2, CorrectAttempt=5,  TotalAttempt=10, Repetition=0 },
            };
            progressRepoMock
                .Setup(r => r.GetAllUserVocabularyProgressByUserAsync(1, default))
                .ReturnsAsync(progresses);

            // ACT
            var score = await service.GetOverallRetentionScoreAsync(1);

            // ASSERT
            score.Should().Be(75);
        }

        #endregion
    }
}
