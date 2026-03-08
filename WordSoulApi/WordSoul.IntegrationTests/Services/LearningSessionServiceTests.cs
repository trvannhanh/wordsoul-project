using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using WordSoul.Application.DTOs.AnswerRecord;
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
                _timeProvider

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
                VocabularyId = vocab.Id,
                Answer = vocab.Word,
                QuestionType = QuestionType.FillInBlank,
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
                    VocabularyId = vocab.Id,
                    Answer = vocab.Word,
                    QuestionType = QuestionType.FillInBlank,
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
                VocabularyId = vocab.Id,
                Answer = "wrong",
                QuestionType = QuestionType.FillInBlank,
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
                VocabularyId = vocab.Id,
                Answer = vocab.Word,
                QuestionType = QuestionType.FillInBlank,
                HintCount = 2,
                ResponseTimeSeconds = 2
            };

            await _service.SubmitAnswerAsync(user.Id, session.Id, request);

            var history = await _context.VocabularyReviewHistories.FirstAsync();
            history.Grade.Should().BeLessThan(5);
        }

        // ======================================================
        // HELPER: Setup Review Session
        // ======================================================
        private async Task<(User user, Vocabulary vocab, LearningSession session)>
            SetupReviewSession()
        {
            var user = await _dataBuilder.CreateUserAsync("review_user");
            var vocab = await _dataBuilder.CreateVocabularyAsync("apple", "quả táo");

            var session = new LearningSession
            {
                UserId = user.Id,
                Type = SessionType.Review,
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
                CurrentLevel = 3,
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
    }
}
