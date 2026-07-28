using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using WordSoul.Application.Common;
using WordSoul.Application.DTOs.AnswerRecord;
using WordSoul.Application.DTOs.Pet;
using WordSoul.Application.DTOs.SRS;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Application.Learning.QuestionFlow;
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

        public static TheoryData<SessionType, int, QuestionType> CurrentQuestionFlowCases => new()
        {
            { SessionType.Learning, 0, QuestionType.Flashcard },
            { SessionType.Learning, 1, QuestionType.FillInBlank },
            { SessionType.Learning, 2, QuestionType.MultipleChoice },
            { SessionType.Learning, 3, QuestionType.Listening },
            { SessionType.Review, 0, QuestionType.Flashcard },
            { SessionType.Review, 1, QuestionType.FillInBlank },
            { SessionType.Review, 2, QuestionType.MultipleChoice },
            { SessionType.Review, 3, QuestionType.Listening }
        };

        public static TheoryData<int, QuestionType, string, bool> CurrentAnswerEvaluationCases => new()
        {
            { 0, QuestionType.Flashcard, "anything", true },
            { 1, QuestionType.FillInBlank, "  EPHEMERAL  ", true },
            { 2, QuestionType.MultipleChoice, "Ephemeral", true },
            { 2, QuestionType.MultipleChoice, "ephemeral", true },
            { 2, QuestionType.MultipleChoice, "Ｅｐｈｅｍｅｒａｌ", true },
            { 3, QuestionType.Listening, "  EPHEMERAL  ", true }
        };

        public static TheoryData<int, QuestionType, bool> VersionedReviewQuestionFlowCases => new()
        {
            { 0, QuestionType.FillInBlank, false },
            { 1, QuestionType.Flashcard, true },
            { 2, QuestionType.Listening, true }
        };

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
            Mock<IActivityLogService> ActivityLogService,
            Mock<IDbContextTransaction> Transaction);

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
            var transaction = new Mock<IDbContextTransaction>();

            // Wire IUnitOfWork properties
            uowMock.SetupGet(x => x.LearningSession).Returns(sessionRepo.Object);
            uowMock.SetupGet(x => x.SessionVocabulary).Returns(sessionVocabRepo.Object);
            uowMock.SetupGet(x => x.AnswerRecord).Returns(answerRecordRepo.Object);
            uowMock.SetupGet(x => x.UserVocabularySet).Returns(userVocabSetRepo.Object);
            uowMock.SetupGet(x => x.SetVocabulary).Returns(setVocabRepo.Object);
            uowMock.SetupGet(x => x.UserVocabularyProgress).Returns(progressRepo.Object);
            uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(1);
            uowMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(transaction.Object);

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
                sysConfig.Object,
                CreateQuestionFlowResolver());

            var deps = new Deps(
                uowMock, sessionRepo, sessionVocabRepo, answerRecordRepo,
                userVocabSetRepo, setVocabRepo, progressRepo,
                srsService, rewardPetService, petBuffService,
                userOwnedPetService, dailyQuestService, gymLeaderService,
                activityLogService, transaction);

            return (service, deps);
        }

        private static IQuestionFlowResolver CreateQuestionFlowResolver() =>
            new QuestionFlowResolver(
                new LegacyQuestionFlowPolicy(),
                new LearningQuestionFlowPolicy(),
                new ReviewQuestionFlowPolicy());

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

        [Fact]
        public async Task CreateLearningSession_NewSession_StartsAtInitialStage()
        {
            var (service, deps) = CreateService();
            LearningSession? createdSession = null;
            deps.SessionRepo
                .Setup(r => r.GetExistingLearningSessionUnCompletedForUserAsync(1, 1, default))
                .ReturnsAsync((LearningSession?)null);
            deps.UserVocabSetRepo
                .Setup(r => r.GetUserVocabularySetAsync(1, 1, default))
                .ReturnsAsync(new UserVocabularySet
                {
                    UserId = 1,
                    VocabularySetId = 1
                });
            deps.SetVocabRepo
                .Setup(r => r.GetUnlearnedVocabulariesFromSetAsync(
                    1,
                    1,
                    It.IsAny<int>(),
                    default))
                .ReturnsAsync(new[]
                {
                    new Vocabulary { Id = 10, Word = "Ephemeral" }
                });
            deps.RewardPetService
                .Setup(s => s.GetRandomPetBySetIdAsync(1, 0, default))
                .ReturnsAsync(new Pet
                {
                    Id = 7,
                    Name = "FlowPet",
                    Rarity = PetRarity.Common
                });
            deps.SessionRepo
                .Setup(r => r.CreateLearningSessionAsync(
                    It.IsAny<LearningSession>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((LearningSession session, CancellationToken _) =>
                {
                    session.Id = 42;
                    createdSession = session;
                    return session;
                });

            var result = await service.CreateLearningSessionAsync(1, 1);

            result.Id.Should().Be(42);
            createdSession.Should().NotBeNull();
            createdSession!.FlowVersion.Should().Be(QuestionFlowVersions.Current);
            createdSession.SessionVocabularies.Should().ContainSingle()
                .Which.CurrentStageIndex.Should().Be(0);
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
        public async Task CompleteSession_EndpointTypeDoesNotMatchStoredType_ThrowsInvalidOperation()
        {
            var (service, deps) = CreateService();
            deps.SessionRepo
                .Setup(r => r.GetLearningSessionByIdAsync(1, default))
                .ReturnsAsync(new LearningSession
                {
                    Id = 1,
                    UserId = 1,
                    Type = SessionType.Learning,
                    IsCompleted = false
                });

            Func<Task> act = () =>
                service.CompleteSessionAsync(userId: 1, sessionId: 1, SessionType.Review);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Session type mismatch*");
            deps.SessionVocabRepo.Verify(
                r => r.GetSessionVocabulariesBySessionIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
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
                .ReturnsAsync(new LearningSession { Id = 1, UserId = 1, IsCompleted = true });

            // ACT
            var result = await service.GetSessionQuestionsAsync(userId: 1, sessionId: 1);

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
                .ReturnsAsync(new LearningSession { Id = 1, UserId = 1, IsCompleted = false });
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
            var result = await service.GetSessionQuestionsAsync(userId: 1, sessionId: 1);

            // ASSERT
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetSessionQuestions_SessionOwnedByAnotherUser_ThrowsUnauthorized()
        {
            var (service, deps) = CreateService();
            deps.SessionRepo
                .Setup(r => r.GetLearningSessionByIdAsync(1, default))
                .ReturnsAsync(new LearningSession { Id = 1, UserId = 99 });

            Func<Task> act = async () =>
                await service.GetSessionQuestionsAsync(userId: 1, sessionId: 1);

            await act.Should().ThrowAsync<UnauthorizedAccessException>();
            deps.SessionVocabRepo.Verify(
                r => r.GetSessionVocabulariesBySessionIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Theory]
        [MemberData(nameof(CurrentQuestionFlowCases))]
        public async Task GetSessionQuestions_CurrentV1Flow_CreatesExpectedQuestionPayload(
            SessionType sessionType,
            int currentLevel,
            QuestionType expectedType)
        {
            var (service, deps) = CreateService();
            var target = new Vocabulary
            {
                Id = 10,
                Word = "Ephemeral",
                Meaning = "lasting for a short time",
                Description = "I saw EPHEMERAL today.",
                Pronunciation = "ih-FEM-er-uhl",
                PronunciationUrl = "https://audio.test/ephemeral.mp3",
                ImageUrl = "https://image.test/ephemeral.png",
                ExampleSentence = "Beauty can be ephemeral.",
                ExampleSentenceAudioUrl = "https://audio.test/example.mp3"
            };

            var sessionVocabularies = new List<SessionVocabulary>
            {
                new()
                {
                    LearningSessionId = 1,
                    VocabularyId = target.Id,
                    Order = 1,
                    CurrentStageIndex = currentLevel,
                    IsCompleted = false,
                    Vocabulary = target
                },
                CompletedSessionVocabulary(11, 2, "Durable"),
                CompletedSessionVocabulary(12, 3, "Permanent"),
                CompletedSessionVocabulary(13, 4, "Endless")
            };

            deps.SessionRepo
                .Setup(r => r.GetLearningSessionByIdAsync(1, default))
                .ReturnsAsync(new LearningSession
                {
                    Id = 1,
                    UserId = 1,
                    Type = sessionType,
                    IsCompleted = false
                });
            deps.SessionVocabRepo
                .Setup(r => r.GetSessionVocabulariesBySessionIdAsync(1, default))
                .ReturnsAsync(sessionVocabularies);

            var question = (await service.GetSessionQuestionsAsync(1, 1))
                .Should().ContainSingle().Which;

            question.VocabularyId.Should().Be(target.Id);
            question.QuestionType.Should().Be(expectedType);
            question.Word.Should().Be(target.Word);
            question.Meaning.Should().Be(target.Meaning);
            question.PronunciationUrl.Should().Be(target.PronunciationUrl);
            question.ImageUrl.Should().Be(target.ImageUrl);
            question.ExampleSentence.Should().Be(target.ExampleSentence);
            question.IsRetry.Should().Be(currentLevel > 0);

            switch (expectedType)
            {
                case QuestionType.Flashcard:
                    question.QuestionPrompt.Should().BeNull();
                    question.Options.Should().BeNull();
                    break;

                case QuestionType.FillInBlank:
                    question.QuestionPrompt.Should().Be("I saw ___ today.");
                    question.Options.Should().BeNull();
                    break;

                case QuestionType.MultipleChoice:
                    question.QuestionPrompt.Should().Be(target.Meaning);
                    question.Options.Should().BeEquivalentTo(
                        "Ephemeral", "Durable", "Permanent", "Endless");
                    break;

                case QuestionType.Listening:
                    question.QuestionPrompt.Should().BeNull();
                    question.Options.Should().BeNull();
                    question.PronunciationUrl.Should().Be("https://audio.test/ephemeral.mp3");
                    break;
            }
        }

        [Theory]
        [MemberData(nameof(VersionedReviewQuestionFlowCases))]
        public async Task GetSessionQuestions_VersionedReview_UsesReviewPolicy(
            int currentStageIndex,
            QuestionType expectedType,
            bool expectedRetry)
        {
            var (service, deps) = CreateService();
            var vocabulary = new Vocabulary
            {
                Id = 10,
                Word = "Ephemeral",
                Meaning = "lasting for a short time",
                Description = "The moment was ephemeral."
            };
            deps.SessionRepo
                .Setup(r => r.GetLearningSessionByIdAsync(1, default))
                .ReturnsAsync(new LearningSession
                {
                    Id = 1,
                    UserId = 1,
                    Type = SessionType.Review,
                    FlowVersion = QuestionFlowVersions.Current,
                    IsCompleted = false
                });
            deps.SessionVocabRepo
                .Setup(r => r.GetSessionVocabulariesBySessionIdAsync(1, default))
                .ReturnsAsync(new[]
                {
                    new SessionVocabulary
                    {
                        LearningSessionId = 1,
                        VocabularyId = vocabulary.Id,
                        Order = 1,
                        CurrentStageIndex = currentStageIndex,
                        IsCompleted = false,
                        Vocabulary = vocabulary
                    }
                });

            var question = (await service.GetSessionQuestionsAsync(1, 1))
                .Should().ContainSingle().Which;

            question.QuestionType.Should().Be(expectedType);
            question.IsRetry.Should().Be(expectedRetry);
        }

        [Fact]
        public async Task GetSessionQuestions_FillInBlankWithoutMatchingDescription_UsesNullPromptFallback()
        {
            var (service, deps) = CreateService();
            var vocabulary = new Vocabulary
            {
                Id = 10,
                Word = "Ephemeral",
                Meaning = "lasting for a short time",
                Description = "This description does not contain the target term."
            };
            deps.SessionRepo
                .Setup(r => r.GetLearningSessionByIdAsync(1, default))
                .ReturnsAsync(new LearningSession
                {
                    Id = 1,
                    UserId = 1,
                    Type = SessionType.Learning,
                    IsCompleted = false
                });
            deps.SessionVocabRepo
                .Setup(r => r.GetSessionVocabulariesBySessionIdAsync(1, default))
                .ReturnsAsync(new List<SessionVocabulary>
                {
                    new()
                    {
                        LearningSessionId = 1,
                        VocabularyId = vocabulary.Id,
                        Order = 1,
                        CurrentStageIndex = 1,
                        IsCompleted = false,
                        Vocabulary = vocabulary
                    }
                });

            var question = (await service.GetSessionQuestionsAsync(1, 1))
                .Should().ContainSingle().Which;

            question.QuestionType.Should().Be(QuestionType.FillInBlank);
            question.QuestionPrompt.Should().BeNull();
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
                SubmissionId = Guid.NewGuid(),
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
                CurrentStageIndex = 0, IsCompleted = false,
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
                SubmissionId = Guid.NewGuid(),
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
                CurrentStageIndex = 1, IsCompleted = false,
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
                SubmissionId = Guid.NewGuid(),
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
            result.NewStageIndex.Should().Be(0);
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
                CurrentStageIndex = 1, IsCompleted = false,
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
                SubmissionId = Guid.NewGuid(),
                VocabularyId = 10,
                QuestionType = QuestionType.FillInBlank,
                Answer = "ephemeral"    // Correct answer (case-insensitive)
            };

            // ACT
            var result = await service.SubmitAnswerAsync(1, 1, request);

            // ASSERT
            result.IsCorrect.Should().BeTrue();
            // Level increases: 2 → 3 (not yet complete, needs 4)
            result.NewStageIndex.Should().Be(2);
            result.IsVocabularyCompleted.Should().BeFalse();
        }

        [Fact]
        public async Task SubmitAnswer_CorrectAnswer_LevelReaches4_VocabCompleted()
        {
            // ARRANGE — CurrentStageIndex=3, correct answer → stage=4 → IsCompleted=true
            var (service, deps) = CreateService();

            var vocab = new Vocabulary { Id = 10, Word = "perseverance", Meaning = "sự kiên trì" };
            var sessionVocab = new SessionVocabulary
            {
                LearningSessionId = 1, VocabularyId = 10,
                CurrentStageIndex = 3, IsCompleted = false,
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
                .Setup(r => r.GetAttemptCountAsync(1, 10, QuestionType.Listening, default))
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
                SubmissionId = Guid.NewGuid(),
                VocabularyId = 10,
                QuestionType = QuestionType.Listening,
                Answer = "perseverance"
            };

            // ACT
            var result = await service.SubmitAnswerAsync(1, 1, request);

            // ASSERT — level=4 → IsCompleted=true
            result.IsCorrect.Should().BeTrue();
            result.NewStageIndex.Should().Be(4);
            result.IsVocabularyCompleted.Should().BeTrue();
        }

        [Fact]
        public async Task SubmitAnswer_QuestionTypeDoesNotMatchCurrentStage_ThrowsInvalidOperation()
        {
            var (service, deps) = CreateService();
            var submissionId = Guid.NewGuid();
            deps.SessionRepo
                .Setup(r => r.GetExistingLearningSessionForUserAsync(1, 1, default))
                .ReturnsAsync(new LearningSession { Id = 1, UserId = 1, IsCompleted = false });
            deps.SessionVocabRepo
                .Setup(r => r.GetSessionVocabularyAsync(1, 10, default))
                .ReturnsAsync(new SessionVocabulary
                {
                    LearningSessionId = 1,
                    VocabularyId = 10,
                    CurrentStageIndex = 2,
                    Vocabulary = new Vocabulary { Id = 10, Word = "secure" }
                });

            var request = new SubmitAnswerRequestDto
            {
                SubmissionId = submissionId,
                VocabularyId = 10,
                QuestionType = QuestionType.Flashcard,
                Answer = "viewed"
            };

            Func<Task> act = () => service.SubmitAnswerAsync(1, 1, request);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Question type mismatch*");
            deps.AnswerRecordRepo.Verify(
                r => r.CreateAnswerRecordAsync(It.IsAny<AnswerRecord>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task SubmitAnswer_CompletedSession_ThrowsInvalidOperation()
        {
            var (service, deps) = CreateService();
            deps.SessionRepo
                .Setup(r => r.GetExistingLearningSessionForUserAsync(1, 1, default))
                .ReturnsAsync(new LearningSession { Id = 1, UserId = 1, IsCompleted = true });

            var request = new SubmitAnswerRequestDto
            {
                SubmissionId = Guid.NewGuid(),
                VocabularyId = 10,
                QuestionType = QuestionType.Flashcard,
                Answer = "viewed"
            };

            Func<Task> act = () => service.SubmitAnswerAsync(1, 1, request);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*completed session*");
            deps.SessionVocabRepo.Verify(
                r => r.GetSessionVocabularyAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task SubmitAnswer_DuplicateSubmission_ReturnsStoredResultWithoutAdvancingProgress()
        {
            var (service, deps) = CreateService();
            var submissionId = Guid.NewGuid();
            deps.SessionRepo
                .Setup(r => r.GetExistingLearningSessionForUserAsync(1, 1, default))
                .ReturnsAsync(new LearningSession { Id = 1, UserId = 1, IsCompleted = true });
            deps.AnswerRecordRepo
                .Setup(r => r.GetBySubmissionIdAsync(1, submissionId, default))
                .ReturnsAsync(new AnswerRecord
                {
                    SubmissionId = submissionId,
                    LearningSessionId = 1,
                    VocabularyId = 10,
                    Vocabulary = new Vocabulary { Id = 10, Word = "secure" },
                    QuestionType = QuestionType.Listening,
                    Answer = "secure",
                    IsCorrect = true,
                    AttemptCount = 2,
                    ResultingLevel = 4,
                    IsVocabularyCompleted = true
                });

            var result = await service.SubmitAnswerAsync(1, 1, new SubmitAnswerRequestDto
            {
                SubmissionId = submissionId,
                VocabularyId = 10,
                QuestionType = QuestionType.Listening,
                Answer = "secure"
            });

            result.IsCorrect.Should().BeTrue();
            result.AttemptNumber.Should().Be(2);
            result.NewStageIndex.Should().Be(4);
            result.IsVocabularyCompleted.Should().BeTrue();
            deps.SessionVocabRepo.Verify(
                r => r.GetSessionVocabularyAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
            deps.Uow.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task SubmitAnswer_Success_CommitsTransaction()
        {
            var (service, deps, request) = CreateAnswerEvaluationScenario(
                0,
                QuestionType.Flashcard,
                "viewed");

            await service.SubmitAnswerAsync(1, 1, request);

            deps.Transaction.Verify(
                transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()),
                Times.Once);
            deps.Transaction.Verify(
                transaction => transaction.RollbackAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task SubmitAnswer_SaveFailure_RollsBackAndClearsTracking()
        {
            var (service, deps, request) = CreateAnswerEvaluationScenario(
                0,
                QuestionType.Flashcard,
                "viewed");
            deps.Uow
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateException("write failed"));

            var act = () => service.SubmitAnswerAsync(1, 1, request);

            await act.Should().ThrowAsync<DbUpdateException>();
            deps.Transaction.Verify(
                transaction => transaction.RollbackAsync(It.IsAny<CancellationToken>()),
                Times.Once);
            deps.Uow.Verify(uow => uow.ClearTrackedChanges(), Times.Once);
        }

        [Fact]
        public async Task SubmitAnswer_ConcurrentStateChange_ReturnsActionableConflict()
        {
            var (service, deps, request) = CreateAnswerEvaluationScenario(
                0,
                QuestionType.Flashcard,
                "viewed");
            deps.Uow
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateConcurrencyException("stale stage"));

            var act = () => service.SubmitAnswerAsync(1, 1, request);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*question state changed*");
            deps.Transaction.Verify(
                transaction => transaction.RollbackAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SubmitAnswer_ConcurrentDuplicate_ReturnsWinningStoredResult()
        {
            var (service, deps, request) = CreateAnswerEvaluationScenario(
                0,
                QuestionType.Flashcard,
                "viewed");
            var storedAnswer = new AnswerRecord
            {
                SubmissionId = request.SubmissionId,
                LearningSessionId = 1,
                VocabularyId = request.VocabularyId,
                Vocabulary = new Vocabulary { Id = request.VocabularyId, Word = "Ephemeral" },
                QuestionType = request.QuestionType,
                Answer = request.Answer,
                IsCorrect = true,
                AttemptCount = 1,
                ResultingLevel = 1,
                IsVocabularyCompleted = false
            };
            deps.AnswerRecordRepo
                .SetupSequence(repository => repository.GetBySubmissionIdAsync(
                    1,
                    request.SubmissionId,
                    default))
                .ReturnsAsync((AnswerRecord?)null)
                .ReturnsAsync((AnswerRecord?)null)
                .ReturnsAsync(storedAnswer);
            deps.Uow
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateConcurrencyException("lost race"));

            var result = await service.SubmitAnswerAsync(1, 1, request);

            result.IsCorrect.Should().BeTrue();
            result.NewStageIndex.Should().Be(1);
            deps.Transaction.Verify(
                transaction => transaction.RollbackAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Theory]
        [MemberData(nameof(CurrentAnswerEvaluationCases))]
        public async Task SubmitAnswer_CurrentEvaluationRules_ReturnExpectedResult(
            int currentLevel,
            QuestionType questionType,
            string answer,
            bool expectedCorrect)
        {
            var (service, deps, request) = CreateAnswerEvaluationScenario(
                currentLevel,
                questionType,
                answer);

            var result = await service.SubmitAnswerAsync(1, 1, request);

            result.IsCorrect.Should().Be(expectedCorrect);
            result.CorrectAnswer.Should().Be("Ephemeral");
            result.NewStageIndex.Should().Be(expectedCorrect
                ? currentLevel + 1
                : Math.Max(0, currentLevel - 1));
        }

        [Fact]
        public async Task SubmitAnswer_TextAnswer_NormalizesRepeatedWhitespace()
        {
            var (service, _, request) = CreateAnswerEvaluationScenario(
                1,
                QuestionType.FillInBlank,
                "  short   term  ",
                "Short Term");

            var result = await service.SubmitAnswerAsync(1, 1, request);

            result.IsCorrect.Should().BeTrue();
        }

        private static SessionVocabulary CompletedSessionVocabulary(
            int vocabularyId,
            int order,
            string word)
        {
            return new SessionVocabulary
            {
                LearningSessionId = 1,
                VocabularyId = vocabularyId,
                Order = order,
                CurrentStageIndex = 4,
                IsCompleted = true,
                Vocabulary = new Vocabulary { Id = vocabularyId, Word = word }
            };
        }

        private static (
            LearningSessionService Service,
            Deps Deps,
            SubmitAnswerRequestDto Request) CreateAnswerEvaluationScenario(
                int currentLevel,
                QuestionType questionType,
                string answer,
                string vocabularyWord = "Ephemeral")
        {
            var (service, deps) = CreateService();
            var vocabulary = new Vocabulary
            {
                Id = 10,
                Word = vocabularyWord,
                Meaning = "lasting for a short time"
            };
            var sessionVocabulary = new SessionVocabulary
            {
                LearningSessionId = 1,
                VocabularyId = vocabulary.Id,
                CurrentStageIndex = currentLevel,
                IsCompleted = false,
                Vocabulary = vocabulary
            };
            var progress = new UserVocabularyProgress
            {
                UserId = 1,
                VocabularyId = vocabulary.Id,
                CorrectAttempt = 0,
                TotalAttempt = 0
            };

            deps.SessionRepo
                .Setup(r => r.GetExistingLearningSessionForUserAsync(1, 1, default))
                .ReturnsAsync(new LearningSession
                {
                    Id = 1,
                    UserId = 1,
                    Type = SessionType.Learning,
                    IsCompleted = false
                });
            deps.SessionVocabRepo
                .Setup(r => r.GetSessionVocabularyAsync(1, vocabulary.Id, default))
                .ReturnsAsync(sessionVocabulary);
            deps.ProgressRepo
                .Setup(r => r.GetUserVocabularyProgressAsync(1, vocabulary.Id, default))
                .ReturnsAsync(progress);
            deps.ProgressRepo
                .Setup(r => r.UpdateSrsParametersAsync(
                    It.IsAny<UserVocabularyProgress>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(progress);
            deps.AnswerRecordRepo
                .Setup(r => r.GetAttemptCountAsync(1, vocabulary.Id, questionType, default))
                .ReturnsAsync(0);
            deps.AnswerRecordRepo
                .Setup(r => r.CreateAnswerRecordAsync(
                    It.IsAny<AnswerRecord>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((AnswerRecord record, CancellationToken _) => record);
            deps.SessionVocabRepo
                .Setup(r => r.UpdateSessionVocabularyAsync(
                    It.IsAny<SessionVocabulary>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(sessionVocabulary);

            return (
                service,
                deps,
                new SubmitAnswerRequestDto
                {
                    SubmissionId = Guid.NewGuid(),
                    VocabularyId = vocabulary.Id,
                    QuestionType = questionType,
                    Answer = answer,
                    ResponseTimeSeconds = 2,
                    HintCount = 0
                });
        }

        #endregion
    }
}
