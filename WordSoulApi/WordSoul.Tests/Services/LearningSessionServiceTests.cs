using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WordSoul.Application.Common;
using WordSoul.Application.DTOs.AnswerRecord;
using WordSoul.Application.DTOs.Pet;
using WordSoul.Application.DTOs.SRS;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Application.Services;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Tests.Services
{
    /// <summary>
    /// Unit tests cho LearningSessionService.
    /// Tập trung vào: validation, early-exit branches, key business logic.
    /// Orchestration phức tạp (happy-path full flow) được bao phủ bởi IntegrationTests.
    /// </summary>
    public class LearningSessionServiceTests
    {
        private static readonly DateTime FakeNow =
            new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);

        // ─── Inline ITimeProvider ─────────────────────────────────────────
        private sealed class FixedTime : ITimeProvider
        {
            public DateTime UtcNow => FakeNow;
        }

        // ─── All mock dependencies in one record ──────────────────────────
        private record Deps(
            Mock<IUnitOfWork> Uow,
            Mock<ILearningSessionRepository> SessionRepo,
            Mock<ISessionVocabularyRepository> SessionVocabRepo,
            Mock<IAnswerRecordRepository> AnswerRecordRepo,
            Mock<IUserVocabularySetRepository> UserVocabSetRepo,
            Mock<ISetVocabularyRepository> SetVocabRepo,
            Mock<IUserVocabularyProgressRepository> ProgressRepo,
            Mock<ISRSService> SrsService,
            Mock<ISetRewardPetService> RewardPetService,
            Mock<IPetBuffService> PetBuffService,
            Mock<IUserOwnedPetService> UserOwnedPetService,
            Mock<IDailyQuestService> DailyQuestService,
            Mock<IGymLeaderService> GymLeaderService,
            Mock<IActivityLogService> ActivityLogService);

        // ─── Factory helper ───────────────────────────────────────────────
        private static (LearningSessionService service, Deps deps) CreateService()
        {
            var uowMock = new Mock<IUnitOfWork>();
            var sessionRepo = new Mock<ILearningSessionRepository>();
            var sessionVocabRepo = new Mock<ISessionVocabularyRepository>();
            var answerRecordRepo = new Mock<IAnswerRecordRepository>();
            var userVocabSetRepo = new Mock<IUserVocabularySetRepository>();
            var setVocabRepo = new Mock<ISetVocabularyRepository>();
            var progressRepo = new Mock<IUserVocabularyProgressRepository>();

            // Wire IUnitOfWork properties
            uowMock.SetupGet(x => x.LearningSession).Returns(sessionRepo.Object);
            uowMock.SetupGet(x => x.SessionVocabulary).Returns(sessionVocabRepo.Object);
            uowMock.SetupGet(x => x.AnswerRecord).Returns(answerRecordRepo.Object);
            uowMock.SetupGet(x => x.UserVocabularySet).Returns(userVocabSetRepo.Object);
            uowMock.SetupGet(x => x.SetVocabulary).Returns(setVocabRepo.Object);
            uowMock.SetupGet(x => x.UserVocabularyProgress).Returns(progressRepo.Object);
            uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(1);

            var srsService = new Mock<ISRSService>();
            var rewardPetService = new Mock<ISetRewardPetService>();
            var petBuffService = new Mock<IPetBuffService>();
            var userOwnedPetService = new Mock<IUserOwnedPetService>();
            var dailyQuestService = new Mock<IDailyQuestService>();
            var gymLeaderService = new Mock<IGymLeaderService>();
            var activityLogService = new Mock<IActivityLogService>();
            var sysConfig = new Mock<ISystemConfigurationService>();

            // Setup sysConfig to return default values
            sysConfig
                .Setup(s => s.GetValueAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string key, int defaultValue, CancellationToken ct) => defaultValue);
            sysConfig
                .Setup(s => s.GetValueAsync(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string key, double defaultValue, CancellationToken ct) => defaultValue);

            // Default: pet buff returns null (no active buff)
            petBuffService
                .Setup(s => s.GetActivePetBuffAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PetBuffDto?)null);

            // Default: activity log fire-and-forget, no throw
            activityLogService
                .Setup(s => s.TrackStartLearningSessionAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            activityLogService
                .Setup(s => s.TrackFinishLearningSessionAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            activityLogService
                .Setup(s => s.TrackVocabularyReviewedAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var service = new LearningSessionService(
                uowMock.Object,
                new Mock<ILogger<LearningSessionService>>().Object,
                userOwnedPetService.Object,
                new Mock<IUserVocabularyProgressService>().Object,
                activityLogService.Object,
                rewardPetService.Object,
                srsService.Object,
                dailyQuestService.Object,
                petBuffService.Object,
                new FixedTime(),
                gymLeaderService.Object,
                sysConfig.Object);

            var deps = new Deps(
                uowMock, sessionRepo, sessionVocabRepo, answerRecordRepo,
                userVocabSetRepo, setVocabRepo, progressRepo,
                srsService, rewardPetService, petBuffService,
                userOwnedPetService, dailyQuestService, gymLeaderService, activityLogService);

            return (service, deps);
        }

        // ──────────────────────────────────────────────────────────────────
        #region CreateLearningSessionAsync — Validation
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateLearningSession_UserId_Zero_ThrowsArgumentException()
        {
            var (service, _) = CreateService();

            Func<Task> act = () => service.CreateLearningSessionAsync(userId: 0, setId: 1);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*UserId*");
        }

        [Fact]
        public async Task CreateLearningSession_SetId_Zero_ThrowsArgumentException()
        {
            var (service, _) = CreateService();

            Func<Task> act = () => service.CreateLearningSessionAsync(userId: 1, setId: 0);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*VocabularySetId*");
        }

        // ──────────────────────────────────────────────────────────────────
        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region CreateLearningSessionAsync — Business Logic
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateLearningSession_ExistingUncompletedSession_ReturnsExisting()
        {
            // ARRANGE
            var (service, deps) = CreateService();

            var existingSession = new LearningSession
            {
                Id = 42,
                UserId = 1,
                IsCompleted = false,
                PetId = 7,
                CatchRate = 0.6,
                PetXpMultiplier = 1.0,
                SessionVocabularies =
                [
                    new SessionVocabulary { VocabularyId = 10 },
                    new SessionVocabulary { VocabularyId = 11 }
                ]
            };

            deps.SessionRepo
                .Setup(r => r.GetExistingLearningSessionUnCompletedForUserAsync(1, 1, default))
                .ReturnsAsync(existingSession);
            deps.AnswerRecordRepo
                .Setup(r => r.GetCorrectAnswerRecordNumberFromSession(42, default))
                .ReturnsAsync(3);

            // ACT
            var result = await service.CreateLearningSessionAsync(userId: 1, setId: 1);

            // ASSERT — return existing session, NOT create a new one
            result.Id.Should().Be(42);
            result.VocabularyIds.Should().BeEquivalentTo(new[] { 10, 11 });
            result.CurrentCorrectAnswered.Should().Be(3);
        }

        [Fact]
        public async Task CreateLearningSession_UserDoesNotOwnSet_ThrowsInvalidOperation()
        {
            // ARRANGE
            var (service, deps) = CreateService();
            deps.SessionRepo
                .Setup(r => r.GetExistingLearningSessionUnCompletedForUserAsync(1, 1, default))
                .ReturnsAsync((LearningSession?)null);
            deps.UserVocabSetRepo
                .Setup(r => r.GetUserVocabularySetAsync(1, 1, default))
                .ReturnsAsync((UserVocabularySet?)null);

            // ACT
            Func<Task> act = () => service.CreateLearningSessionAsync(userId: 1, setId: 1);

            // ASSERT
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*doesn't exist*");
        }

        [Fact]
        public async Task CreateLearningSession_NoUnlearnedVocabularies_ThrowsInvalidOperation()
        {
            // ARRANGE
            var (service, deps) = CreateService();
            deps.SessionRepo
                .Setup(r => r.GetExistingLearningSessionUnCompletedForUserAsync(1, 1, default))
                .ReturnsAsync((LearningSession?)null);
            deps.UserVocabSetRepo
                .Setup(r => r.GetUserVocabularySetAsync(1, 1, default))
                .ReturnsAsync(new UserVocabularySet { UserId = 1, VocabularySetId = 1 });
            deps.SetVocabRepo
                .Setup(r => r.GetUnlearnedVocabulariesFromSetAsync(1, 1, It.IsAny<int>(), default))
                .ReturnsAsync(Enumerable.Empty<Vocabulary>());

            // ACT
            Func<Task> act = () => service.CreateLearningSessionAsync(userId: 1, setId: 1);

            // ASSERT
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*No unlearned vocabularies*");
        }

        [Fact]
        public async Task CreateLearningSession_NoPetInSet_ThrowsInvalidOperation()
        {
            // ARRANGE
            var (service, deps) = CreateService();
            deps.SessionRepo
                .Setup(r => r.GetExistingLearningSessionUnCompletedForUserAsync(1, 1, default))
                .ReturnsAsync((LearningSession?)null);
            deps.UserVocabSetRepo
                .Setup(r => r.GetUserVocabularySetAsync(1, 1, default))
                .ReturnsAsync(new UserVocabularySet { UserId = 1, VocabularySetId = 1 });
            deps.SetVocabRepo
                .Setup(r => r.GetUnlearnedVocabulariesFromSetAsync(1, 1, It.IsAny<int>(), default))
                .ReturnsAsync(new[] { new Vocabulary { Id = 5, Word = "test" } });
            deps.RewardPetService
                .Setup(s => s.GetRandomPetBySetIdAsync(1, It.IsAny<int>(), default))
                .ReturnsAsync((Pet?)null);

            // ACT
            Func<Task> act = () => service.CreateLearningSessionAsync(userId: 1, setId: 1);

            // ASSERT
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*No pets available*");
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region CreateReviewingSessionAsync — Validation & Logic
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateReviewingSession_UserId_Zero_ThrowsArgumentException()
        {
            var (service, _) = CreateService();

            Func<Task> act = () => service.CreateReviewingSessionAsync(userId: 0);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*UserId*");
        }

        [Fact]
        public async Task CreateReviewingSession_NoDueVocabularies_ThrowsInvalidOperation()
        {
            // ARRANGE
            var (service, deps) = CreateService();
            deps.SessionRepo
                .Setup(r => r.GetExistingReviewSessionUnCompletedForUserAsync(1, default))
                .ReturnsAsync((LearningSession?)null);
            deps.SrsService
                .Setup(s => s.GetDueVocabulariesAsync(1, It.IsAny<int>(), default))
                .ReturnsAsync(new List<VocabularyDueDto>());

            // ACT
            Func<Task> act = () => service.CreateReviewingSessionAsync(userId: 1);

            // ASSERT
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*No vocabularies due*");
        }

        [Fact]
        public async Task CreateReviewingSession_ExistingUncompletedSession_ReturnsExisting()
        {
            // ARRANGE
            var (service, deps) = CreateService();

            var existingSession = new LearningSession
            {
                Id = 99,
                UserId = 1,
                IsCompleted = false,
                PetId = null,
                CatchRate = null,
                SessionVocabularies = [new SessionVocabulary { VocabularyId = 20 }]
            };
            deps.SessionRepo
                .Setup(r => r.GetExistingReviewSessionUnCompletedForUserAsync(1, default))
                .ReturnsAsync(existingSession);
            deps.AnswerRecordRepo
                .Setup(r => r.GetCorrectAnswerRecordNumberFromSession(99, default))
                .ReturnsAsync(0);

            // ACT
            var result = await service.CreateReviewingSessionAsync(userId: 1);

            // ASSERT
            result.Id.Should().Be(99);
            result.IsCompleted.Should().BeFalse();
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region CompleteSessionAsync — Validation & Logic
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public async Task CompleteSession_UserId_Zero_ThrowsArgumentException()
        {
            var (service, _) = CreateService();

            Func<Task> act = () =>
                service.CompleteSessionAsync(userId: 0, sessionId: 1, SessionType.Learning);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*UserId*");
        }

        [Fact]
        public async Task CompleteSession_SessionId_Zero_ThrowsArgumentException()
        {
            var (service, _) = CreateService();

            Func<Task> act = () =>
                service.CompleteSessionAsync(userId: 1, sessionId: 0, SessionType.Learning);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*SessionId*");
        }

        [Fact]
        public async Task CompleteSession_SessionNotFoundOrWrongUser_ThrowsUnauthorizedAccess()
        {
            // ARRANGE — session belongs to a different user
            var (service, deps) = CreateService();
            deps.SessionRepo
                .Setup(r => r.GetLearningSessionByIdAsync(1, default))
                .ReturnsAsync(new LearningSession { Id = 1, UserId = 999, IsCompleted = false });

            // ACT
            Func<Task> act = () =>
                service.CompleteSessionAsync(userId: 1, sessionId: 1, SessionType.Learning);

            // ASSERT
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task CompleteSession_SessionAlreadyCompleted_ThrowsInvalidOperation()
        {
            // ARRANGE
            var (service, deps) = CreateService();
            deps.SessionRepo
                .Setup(r => r.GetLearningSessionByIdAsync(1, default))
                .ReturnsAsync(new LearningSession { Id = 1, UserId = 1, IsCompleted = true });

            // ACT
            Func<Task> act = () =>
                service.CompleteSessionAsync(userId: 1, sessionId: 1, SessionType.Learning);

            // ASSERT
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*already completed*");
        }

        [Fact]
        public async Task CompleteSession_NotAllVocabsCompleted_ThrowsInvalidOperation()
        {
            // ARRANGE
            var (service, deps) = CreateService();
            deps.SessionRepo
                .Setup(r => r.GetLearningSessionByIdAsync(1, default))
                .ReturnsAsync(new LearningSession { Id = 1, UserId = 1, IsCompleted = false });
            deps.SessionVocabRepo
                .Setup(r => r.GetSessionVocabulariesBySessionIdAsync(1, default))
                .ReturnsAsync(new List<SessionVocabulary>
                {
                    new() { VocabularyId = 1, IsCompleted = true },
                    new() { VocabularyId = 2, IsCompleted = false }  // NOT yet done
                });

            // ACT
            Func<Task> act = () =>
                service.CompleteSessionAsync(userId: 1, sessionId: 1, SessionType.Learning);

            // ASSERT
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Not all vocabularies*");
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region GetSessionQuestionsAsync
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetSessionQuestions_CompletedSession_ReturnsEmpty()
        {
            // ARRANGE — session.IsCompleted=true → short-circuit
            var (service, deps) = CreateService();
            deps.SessionRepo
                .Setup(r => r.GetLearningSessionByIdAsync(1, default))
                .ReturnsAsync(new LearningSession { Id = 1, IsCompleted = true });

            // ACT
            var result = await service.GetSessionQuestionsAsync(sessionId: 1);

            // ASSERT
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetSessionQuestions_AllVocabsAlreadyCompleted_ReturnsEmpty()
        {
            // ARRANGE — all sessionVocabs have IsCompleted=true
            var (service, deps) = CreateService();
            deps.SessionRepo
                .Setup(r => r.GetLearningSessionByIdAsync(1, default))
                .ReturnsAsync(new LearningSession { Id = 1, IsCompleted = false });
            deps.SessionVocabRepo
                .Setup(r => r.GetSessionVocabulariesBySessionIdAsync(1, default))
                .ReturnsAsync(new List<SessionVocabulary>
                {
                    new() { VocabularyId = 1, IsCompleted = true,
                            Vocabulary = new Vocabulary { Id = 1, Word = "done" } },
                    new() { VocabularyId = 2, IsCompleted = true,
                            Vocabulary = new Vocabulary { Id = 2, Word = "also done" } }
                });

            // ACT
            var result = await service.GetSessionQuestionsAsync(sessionId: 1);

            // ASSERT
            result.Should().BeEmpty();
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────
        #region SubmitAnswerAsync
        // ──────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SubmitAnswer_NullRequest_ThrowsArgumentException()
        {
            var (service, _) = CreateService();

            Func<Task> act = () => service.SubmitAnswerAsync(1, 1, null!);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Invalid request data*");
        }

        [Fact]
        public async Task SubmitAnswer_VocabularyId_Zero_ThrowsArgumentException()
        {
            var (service, _) = CreateService();
            var request = new SubmitAnswerRequestDto
            {
                VocabularyId = 0,
                QuestionType = QuestionType.Flashcard,
                Answer = "anything"
            };

            Func<Task> act = () => service.SubmitAnswerAsync(1, 1, request);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*Invalid request data*");
        }

        [Fact]
        public async Task SubmitAnswer_Flashcard_AlwaysReturnsCorrect()
        {
            // ARRANGE — Flashcard always correct regardless of Answer
            var (service, deps) = CreateService();

            var vocab = new Vocabulary { Id = 10, Word = "ephemeral", Meaning = "tạm thời" };
            var sessionVocab = new SessionVocabulary
            {
                LearningSessionId = 1, VocabularyId = 10,
                CurrentLevel = 0, IsCompleted = false,
                Vocabulary = vocab
            };
            var session = new LearningSession
            {
                Id = 1, UserId = 1,
                Type = SessionType.Learning,
                IsCompleted = false,
                CatchRate = 0.8,
                PetReducePenalty = false
            };
            var progress = new UserVocabularyProgress
            {
                UserId = 1, VocabularyId = 10,
                CorrectAttempt = 5, TotalAttempt = 8
            };

            deps.SessionRepo
                .Setup(r => r.GetExistingLearningSessionForUserAsync(1, 1, default))
                .ReturnsAsync(session);
            deps.SessionVocabRepo
                .Setup(r => r.GetSessionVocabularyAsync(1, 10, default))
                .ReturnsAsync(sessionVocab);
            deps.ProgressRepo
                .Setup(r => r.GetUserVocabularyProgressAsync(1, 10, default))
                .ReturnsAsync(progress);
            deps.AnswerRecordRepo
                .Setup(r => r.GetAttemptCountAsync(1, 10, QuestionType.Flashcard, default))
                .ReturnsAsync(0);
            deps.AnswerRecordRepo
                .Setup(r => r.CreateAnswerRecordAsync(
                    It.IsAny<AnswerRecord>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((AnswerRecord ar, CancellationToken _) => ar);
            deps.SessionVocabRepo
                .Setup(r => r.UpdateSessionVocabularyAsync(
                    It.IsAny<SessionVocabulary>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sessionVocab);

            var request = new SubmitAnswerRequestDto
            {
                VocabularyId = 10,
                QuestionType = QuestionType.Flashcard,
                Answer = "wrong answer doesn't matter"
            };

            // ACT
            var result = await service.SubmitAnswerAsync(1, 1, request);

            // ASSERT — Flashcard is always correct
            result.IsCorrect.Should().BeTrue();
            result.CorrectAnswer.Should().Be("ephemeral");
        }

        [Fact]
        public async Task SubmitAnswer_WrongFillInBlank_IsCorrectFalse_LevelDecreases()
        {
            // ARRANGE
            var (service, deps) = CreateService();

            var vocab = new Vocabulary { Id = 10, Word = "ephemeral", Meaning = "tạm thời" };
            var sessionVocab = new SessionVocabulary
            {
                LearningSessionId = 1, VocabularyId = 10,
                CurrentLevel = 2, IsCompleted = false,
                Vocabulary = vocab
            };
            var session = new LearningSession
            {
                Id = 1, UserId = 1,
                Type = SessionType.Learning,
                IsCompleted = false,
                CatchRate = 0.8,
                PetReducePenalty = false
            };
            var progress = new UserVocabularyProgress
            {
                UserId = 1, VocabularyId = 10,
                CorrectAttempt = 5, TotalAttempt = 8
            };

            deps.SessionRepo
                .Setup(r => r.GetExistingLearningSessionForUserAsync(1, 1, default))
                .ReturnsAsync(session);
            deps.SessionVocabRepo
                .Setup(r => r.GetSessionVocabularyAsync(1, 10, default))
                .ReturnsAsync(sessionVocab);
            deps.ProgressRepo
                .Setup(r => r.GetUserVocabularyProgressAsync(1, 10, default))
                .ReturnsAsync(progress);
            deps.AnswerRecordRepo
                .Setup(r => r.GetAttemptCountAsync(1, 10, QuestionType.FillInBlank, default))
                .ReturnsAsync(0);
            deps.AnswerRecordRepo
                .Setup(r => r.CreateAnswerRecordAsync(
                    It.IsAny<AnswerRecord>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((AnswerRecord ar, CancellationToken _) => ar);
            deps.SessionVocabRepo
                .Setup(r => r.UpdateSessionVocabularyAsync(
                    It.IsAny<SessionVocabulary>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sessionVocab);

            var request = new SubmitAnswerRequestDto
            {
                VocabularyId = 10,
                QuestionType = QuestionType.FillInBlank,
                Answer = "definitely-wrong"   // Wrong answer
            };

            // ACT
            var result = await service.SubmitAnswerAsync(1, 1, request);

            // ASSERT
            result.IsCorrect.Should().BeFalse();
            result.CorrectAnswer.Should().Be("ephemeral");
            // Level decreases: 2 → max(0, 2-1) = 1
            result.NewLevel.Should().Be(1);
            result.IsVocabularyCompleted.Should().BeFalse();
        }

        [Fact]
        public async Task SubmitAnswer_CorrectFillInBlank_LevelIncreases()
        {
            // ARRANGE
            var (service, deps) = CreateService();

            var vocab = new Vocabulary { Id = 10, Word = "ephemeral", Meaning = "tạm thời" };
            var sessionVocab = new SessionVocabulary
            {
                LearningSessionId = 1, VocabularyId = 10,
                CurrentLevel = 2, IsCompleted = false,
                Vocabulary = vocab
            };
            var session = new LearningSession
            {
                Id = 1, UserId = 1,
                Type = SessionType.Learning,
                IsCompleted = false
            };
            var progress = new UserVocabularyProgress
            {
                UserId = 1, VocabularyId = 10,
                CorrectAttempt = 5, TotalAttempt = 8
            };

            deps.SessionRepo
                .Setup(r => r.GetExistingLearningSessionForUserAsync(1, 1, default))
                .ReturnsAsync(session);
            deps.SessionVocabRepo
                .Setup(r => r.GetSessionVocabularyAsync(1, 10, default))
                .ReturnsAsync(sessionVocab);
            deps.ProgressRepo
                .Setup(r => r.GetUserVocabularyProgressAsync(1, 10, default))
                .ReturnsAsync(progress);
            deps.AnswerRecordRepo
                .Setup(r => r.GetAttemptCountAsync(1, 10, QuestionType.FillInBlank, default))
                .ReturnsAsync(0);
            deps.AnswerRecordRepo
                .Setup(r => r.CreateAnswerRecordAsync(
                    It.IsAny<AnswerRecord>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((AnswerRecord ar, CancellationToken _) => ar);
            deps.SessionVocabRepo
                .Setup(r => r.UpdateSessionVocabularyAsync(
                    It.IsAny<SessionVocabulary>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sessionVocab);

            var request = new SubmitAnswerRequestDto
            {
                VocabularyId = 10,
                QuestionType = QuestionType.FillInBlank,
                Answer = "ephemeral"    // Correct answer (case-insensitive)
            };

            // ACT
            var result = await service.SubmitAnswerAsync(1, 1, request);

            // ASSERT
            result.IsCorrect.Should().BeTrue();
            // Level increases: 2 → 3 (not yet complete, needs 4)
            result.NewLevel.Should().Be(3);
            result.IsVocabularyCompleted.Should().BeFalse();
        }

        [Fact]
        public async Task SubmitAnswer_CorrectAnswer_LevelReaches4_VocabCompleted()
        {
            // ARRANGE — CurrentLevel=3, correct answer → level=4 → IsCompleted=true
            var (service, deps) = CreateService();

            var vocab = new Vocabulary { Id = 10, Word = "perseverance", Meaning = "sự kiên trì" };
            var sessionVocab = new SessionVocabulary
            {
                LearningSessionId = 1, VocabularyId = 10,
                CurrentLevel = 3, IsCompleted = false,
                Vocabulary = vocab
            };
            var session = new LearningSession
            {
                Id = 1, UserId = 1,
                Type = SessionType.Learning,
                IsCompleted = false
            };
            var progress = new UserVocabularyProgress
            {
                UserId = 1, VocabularyId = 10,
                CorrectAttempt = 8, TotalAttempt = 10
            };

            deps.SessionRepo
                .Setup(r => r.GetExistingLearningSessionForUserAsync(1, 1, default))
                .ReturnsAsync(session);
            deps.SessionVocabRepo
                .Setup(r => r.GetSessionVocabularyAsync(1, 10, default))
                .ReturnsAsync(sessionVocab);
            deps.ProgressRepo
                .Setup(r => r.GetUserVocabularyProgressAsync(1, 10, default))
                .ReturnsAsync(progress);
            deps.AnswerRecordRepo
                .Setup(r => r.GetAttemptCountAsync(1, 10, QuestionType.Flashcard, default))
                .ReturnsAsync(0);
            deps.AnswerRecordRepo
                .Setup(r => r.CreateAnswerRecordAsync(
                    It.IsAny<AnswerRecord>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((AnswerRecord ar, CancellationToken _) => ar);
            deps.SessionVocabRepo
                .Setup(r => r.UpdateSessionVocabularyAsync(
                    It.IsAny<SessionVocabulary>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(sessionVocab);

            var request = new SubmitAnswerRequestDto
            {
                VocabularyId = 10,
                QuestionType = QuestionType.Flashcard,
                Answer = "perseverance"
            };

            // ACT
            var result = await service.SubmitAnswerAsync(1, 1, request);

            // ASSERT — level=4 → IsCompleted=true
            result.IsCorrect.Should().BeTrue();
            result.NewLevel.Should().Be(4);
            result.IsVocabularyCompleted.Should().BeTrue();
        }

        #endregion
    }
}
