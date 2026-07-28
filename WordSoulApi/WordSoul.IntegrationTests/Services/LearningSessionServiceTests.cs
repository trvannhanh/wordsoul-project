using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using WordSoul.Application.DTOs.AnswerRecord;
using WordSoul.Application.DTOs.SRS;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Application.Learning.QuestionFlow;
using WordSoul.Application.Services;
using WordSoul.Application.Services.SRS;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;
using WordSoul.IntegrationTests.Fakes;

namespace WordSoul.IntegrationTests.Services
{
    public class LearningSessionServiceTests : IntegrationTestBase
    {
        private readonly LearningSessionService _service;
        private readonly FakeTimeProvider _timeProvider;

        public LearningSessionServiceTests()
        {
            _timeProvider = new FakeTimeProvider
            {
                UtcNow = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            var srsService = new SRSService(
                _unitOfWork,
                _srsAlgorithm,
                NullLogger<SRSService>.Instance,
                _timeProvider
            );

            _service = new LearningSessionService(
                _unitOfWork,
                NullLogger<LearningSessionService>.Instance,
                new FakeUserOwnedPetService(),
                new FakeUserVocabularyProgressService(),
                new FakeActivityLogService(),
                new FakeSetRewardPetService(),
                srsService,
                new FakeDailyQuestService(),
                new FakePetBuffService(),
                _timeProvider,
                new FakeGymLeaderService(),
                new FakeSystemConfigurationService(),
                CreateQuestionFlowResolver()
            );
        }

        // ======================================================
        // CASE 1 — PERFECT RECALL → Grade = 5
        // ======================================================
        [Fact]
        public async Task SubmitAnswer_PerfectRecall_ShouldCreateHistory_WithGrade5()
        {
            var (user, vocab, session) = await SetupReviewSession();

            var request = new SubmitAnswerRequestDto
            {
                SubmissionId = Guid.NewGuid(),
                VocabularyId = vocab.Id,
                Answer = vocab.Word,
                QuestionType = QuestionType.Listening,
                HintCount = 0,
                ResponseTimeSeconds = 2
            };

            await _service.SubmitAnswerAsync(user.Id, session.Id, request);

            var history = await _context.VocabularyReviewHistories.FirstOrDefaultAsync();

            history.Should().NotBeNull();
            history!.Grade.Should().Be(5);
            history.IntervalAfter.Should().BeGreaterThan(history.IntervalBefore);
        }

        // ======================================================
        // CASE 2 — ONE FINAL CORRECT AT LEVEL 3 → Grade = 5
        // ======================================================
        [Fact]
        public async Task SubmitAnswer_CompleteFromLevel3_ShouldCreateHistory()
        {
            var (user, vocab, session) = await SetupReviewSession();

            // Chỉ trả lời đúng 1 lần để từ level 3 -> 4
            await _service.SubmitAnswerAsync(user.Id, session.Id,
                new SubmitAnswerRequestDto
                {
                    SubmissionId = Guid.NewGuid(),
                    VocabularyId = vocab.Id,
                    Answer = vocab.Word,
                    QuestionType = QuestionType.Listening,
                    HintCount = 0,
                    ResponseTimeSeconds = 4
                });

            var history = await _context.VocabularyReviewHistories.FirstAsync();

            history.Should().NotBeNull();
            history.IntervalAfter.Should().BeGreaterThan(history.IntervalBefore);
        }

        // ======================================================
        // CASE 3 — FAIL → NO HISTORY
        // ======================================================
        [Fact]
        public async Task SubmitAnswer_Failed_ShouldNotCreateHistory()
        {
            var (user, vocab, session) = await SetupReviewSession();

            var request = new SubmitAnswerRequestDto
            {
                SubmissionId = Guid.NewGuid(),
                VocabularyId = vocab.Id,
                Answer = "wrong",
                QuestionType = QuestionType.Listening,
                HintCount = 0,
                ResponseTimeSeconds = 5
            };

            await _service.SubmitAnswerAsync(user.Id, session.Id, request);

            _context.VocabularyReviewHistories.Should().BeEmpty();
        }

        // ======================================================
        // CASE 4 — HINT USED → Grade < 5
        // ======================================================
        [Fact]
        public async Task SubmitAnswer_WithHints_ShouldLowerGrade()
        {
            var (user, vocab, session) = await SetupReviewSession();

            var request = new SubmitAnswerRequestDto
            {
                SubmissionId = Guid.NewGuid(),
                VocabularyId = vocab.Id,
                Answer = vocab.Word,
                QuestionType = QuestionType.Listening,
                HintCount = 2,
                ResponseTimeSeconds = 2
            };

            await _service.SubmitAnswerAsync(user.Id, session.Id, request);

            var history = await _context.VocabularyReviewHistories.FirstAsync();
            history.Grade.Should().BeLessThan(5);
        }

        [Fact]
        public async Task SubmitAnswer_VersionedReview_FirstRecallCorrect_CompletesWithLockedGrade()
        {
            var (user, vocab, session) = await SetupReviewSession(
                QuestionFlowVersions.Current,
                currentStageIndex: 0);

            var result = await _service.SubmitAnswerAsync(
                user.Id,
                session.Id,
                new SubmitAnswerRequestDto
                {
                    SubmissionId = Guid.NewGuid(),
                    VocabularyId = vocab.Id,
                    Answer = vocab.Word,
                    QuestionType = QuestionType.FillInBlank,
                    HintCount = 0,
                    ResponseTimeSeconds = 2
                });

            result.IsVocabularyCompleted.Should().BeTrue();
            result.NewStageIndex.Should().Be(3);

            var sessionVocabulary = await _context.SessionVocabularies
                .AsNoTracking()
                .SingleAsync(item =>
                    item.LearningSessionId == session.Id
                    && item.VocabularyId == vocab.Id);
            sessionVocabulary.InitialRecallCorrect.Should().BeTrue();
            sessionVocabulary.InitialRecallGrade.Should().Be(5);

            var history = await _context.VocabularyReviewHistories
                .AsNoTracking()
                .SingleAsync();
            history.IsCorrect.Should().BeTrue();
            history.Grade.Should().Be(5);
            history.ResponseTimeSeconds.Should().Be(2);
            history.HintCount.Should().Be(0);

            var progress = await _context.UserVocabularyProgresses
                .AsNoTracking()
                .SingleAsync(item =>
                    item.UserId == user.Id
                    && item.VocabularyId == vocab.Id);
            progress.TotalAttempt.Should().Be(1);
            progress.CorrectAttempt.Should().Be(1);
        }

        [Fact]
        public async Task SubmitAnswer_VersionedReview_FailedRecall_UsesLockedGradeAfterRemediation()
        {
            var (user, vocab, session) = await SetupReviewSession(
                QuestionFlowVersions.Current,
                currentStageIndex: 0);

            var initialResult = await _service.SubmitAnswerAsync(
                user.Id,
                session.Id,
                new SubmitAnswerRequestDto
                {
                    SubmissionId = Guid.NewGuid(),
                    VocabularyId = vocab.Id,
                    Answer = "wrong",
                    QuestionType = QuestionType.FillInBlank,
                    HintCount = 0,
                    ResponseTimeSeconds = 7
                });

            initialResult.IsVocabularyCompleted.Should().BeFalse();
            initialResult.NewStageIndex.Should().Be(1);
            _context.VocabularyReviewHistories.Should().BeEmpty();

            var feedbackResult = await _service.SubmitAnswerAsync(
                user.Id,
                session.Id,
                new SubmitAnswerRequestDto
                {
                    SubmissionId = Guid.NewGuid(),
                    VocabularyId = vocab.Id,
                    Answer = string.Empty,
                    QuestionType = QuestionType.Flashcard,
                    HintCount = 0,
                    ResponseTimeSeconds = 1
                });

            feedbackResult.IsVocabularyCompleted.Should().BeFalse();
            feedbackResult.NewStageIndex.Should().Be(2);
            _context.VocabularyReviewHistories.Should().BeEmpty();

            var correctiveResult = await _service.SubmitAnswerAsync(
                user.Id,
                session.Id,
                new SubmitAnswerRequestDto
                {
                    SubmissionId = Guid.NewGuid(),
                    VocabularyId = vocab.Id,
                    Answer = vocab.Word,
                    QuestionType = QuestionType.Listening,
                    HintCount = 0,
                    ResponseTimeSeconds = 2
                });

            correctiveResult.IsVocabularyCompleted.Should().BeTrue();
            correctiveResult.NewStageIndex.Should().Be(3);

            var sessionVocabulary = await _context.SessionVocabularies
                .AsNoTracking()
                .SingleAsync(item =>
                    item.LearningSessionId == session.Id
                    && item.VocabularyId == vocab.Id);
            sessionVocabulary.InitialRecallCorrect.Should().BeFalse();
            sessionVocabulary.InitialRecallGrade.Should().Be(2);

            var history = await _context.VocabularyReviewHistories
                .AsNoTracking()
                .SingleAsync();
            history.IsCorrect.Should().BeFalse();
            history.Grade.Should().Be(2);
            history.ResponseTimeSeconds.Should().Be(7);
            history.HintCount.Should().Be(0);

            var progress = await _context.UserVocabularyProgresses
                .AsNoTracking()
                .SingleAsync(item =>
                    item.UserId == user.Id
                    && item.VocabularyId == vocab.Id);
            progress.LastGrade.Should().Be(2);
            progress.Repetition.Should().Be(0);
            progress.TotalAttempt.Should().Be(2);
            progress.CorrectAttempt.Should().Be(1);
            progress.WrongCount.Should().Be(1);
        }

        [Fact]
        public async Task SubmitAnswer_DownstreamFailure_RollsBackAnswerAndProgression()
        {
            var (user, vocab, session) = await SetupReviewSession();
            var initialConcurrencyToken = await _context.SessionVocabularies
                .Where(item => item.LearningSessionId == session.Id
                    && item.VocabularyId == vocab.Id)
                .Select(item => item.ConcurrencyToken)
                .SingleAsync();
            var service = new LearningSessionService(
                _unitOfWork,
                NullLogger<LearningSessionService>.Instance,
                new FakeUserOwnedPetService(),
                new FakeUserVocabularyProgressService(),
                new FakeActivityLogService(),
                new FakeSetRewardPetService(),
                new ThrowingSrsService(),
                new FakeDailyQuestService(),
                new FakePetBuffService(),
                _timeProvider,
                new FakeGymLeaderService(),
                new FakeSystemConfigurationService(),
                CreateQuestionFlowResolver());

            var request = new SubmitAnswerRequestDto
            {
                SubmissionId = Guid.NewGuid(),
                VocabularyId = vocab.Id,
                Answer = vocab.Word,
                QuestionType = QuestionType.Listening,
                ResponseTimeSeconds = 2
            };

            var act = () => service.SubmitAnswerAsync(user.Id, session.Id, request);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("forced SRS failure");

            var persistedSessionVocabulary = await _context.SessionVocabularies
                .AsNoTracking()
                .SingleAsync(item => item.LearningSessionId == session.Id
                    && item.VocabularyId == vocab.Id);
            persistedSessionVocabulary.CurrentStageIndex.Should().Be(3);
            persistedSessionVocabulary.IsCompleted.Should().BeFalse();
            persistedSessionVocabulary.ConcurrencyToken
                .Should().Be(initialConcurrencyToken);
            var answerCount = await _context.AnswerRecords
                .AsNoTracking()
                .CountAsync(answer => answer.LearningSessionId == session.Id);
            answerCount.Should().Be(0);
            _context.VocabularyReviewHistories.Should().BeEmpty();
        }

        // ======================================================
        // HELPER: Setup Review Session
        // ======================================================
        private async Task<(User user, Vocabulary vocab, LearningSession session)>
            SetupReviewSession(
                int flowVersion = QuestionFlowVersions.Legacy,
                int currentStageIndex = 3)
        {
            var user = await _dataBuilder.CreateUserAsync("review_user");
            var vocab = await _dataBuilder.CreateVocabularyAsync("apple", "quả táo");

            var session = new LearningSession
            {
                UserId = user.Id,
                Type = SessionType.Review,
                FlowVersion = flowVersion,
                StartTime = _timeProvider.UtcNow,
                IsCompleted = false
            };

            _context.LearningSessions.Add(session);
            await _context.SaveChangesAsync();

            var sessionVocab = new SessionVocabulary
            {
                LearningSessionId = session.Id,
                VocabularyId = vocab.Id,
                Vocabulary = vocab,
                CurrentStageIndex = currentStageIndex,
                IsCompleted = false,
                Order = 1
            };

            var progress = new UserVocabularyProgress
            {
                UserId = user.Id,
                VocabularyId = vocab.Id,
                EasinessFactor = 2.5,
                Interval = 1,
                Repetition = 1,
                NextReviewTime = _timeProvider.UtcNow
            };

            _context.AddRange(sessionVocab, progress);
            await _context.SaveChangesAsync();

            return (user, vocab, session);
        }

        private sealed class ThrowingSrsService : ISRSService
        {
            public Task<SRSUpdateResult> UpdateAfterReviewAsync(
                int userId,
                int vocabularyId,
                int grade,
                CancellationToken ct = default,
                bool recordAttempt = true)
            {
                throw new InvalidOperationException("forced SRS failure");
            }

            public Task<List<VocabularyDueDto>> GetDueVocabulariesAsync(
                int userId,
                int limit = 20,
                CancellationToken ct = default) => throw new NotSupportedException();

            public Task<decimal> GetOverallRetentionScoreAsync(
                int userId,
                CancellationToken ct = default) => throw new NotSupportedException();

            public Task ApplyPronunciationEffectAsync(
                int userId,
                int vocabularyId,
                PronunciationResult result,
                CancellationToken ct = default) => throw new NotSupportedException();
        }

        private static IQuestionFlowResolver CreateQuestionFlowResolver() =>
            new QuestionFlowResolver(
                new LegacyQuestionFlowPolicy(),
                new LearningQuestionFlowPolicy(),
                new ReviewQuestionFlowPolicy());
    }
}
