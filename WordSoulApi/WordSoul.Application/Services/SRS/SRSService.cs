using Microsoft.Extensions.Logging;
using WordSoul.Application.Common;
using WordSoul.Application.DTOs.SRS;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;

namespace WordSoul.Application.Services.SRS
{
    public class SRSService : ISRSService
    {
        private readonly IUnitOfWork _uow;
        private readonly SRSAlgorithm _algorithm;
        private readonly ILogger<SRSService> _logger;
        private readonly ITimeProvider _timeProvider;

        public SRSService(
            IUnitOfWork uow,
            SRSAlgorithm algorithm,
            ILogger<SRSService> logger,
            ITimeProvider timeProvider)
        {
            _uow = uow;
            _algorithm = algorithm;
            _logger = logger;
            _timeProvider = timeProvider;
        }

        public async Task<SRSUpdateResult> UpdateAfterReviewAsync(
            int userId,
            int vocabularyId,
            int grade,
            CancellationToken ct = default)
        {
            // 1. Get current progress
            var progress = await _uow.UserVocabularyProgress
                .GetUserVocabularyProgressAsync(userId, vocabularyId, ct);


            if (progress == null)
            {
                throw new KeyNotFoundException(
                    $"Progress not found for User {userId}, Vocab {vocabularyId}");
            }

            var oldEF = progress.EasinessFactor;
            var oldInterval = progress.Interval;
            var oldRep = progress.Repetition;
            var oldNextReview = progress.NextReviewTime;

            // 2. Save state before update (for history)
            var stateBefore = new
            {
                EF = progress.EasinessFactor,
                Interval = progress.Interval,
                Repetition = progress.Repetition,
                NextReview = progress.NextReviewTime
            };

            // 3. Run SM-2 algorithm
            var srsResult = _algorithm.CalculateNext(
                grade,
                progress.EasinessFactor,
                progress.Interval,
                progress.Repetition,
                _timeProvider.UtcNow
            );

            // 4. Update progress entity
            progress.EasinessFactor = srsResult.NewEaseFactor;
            progress.Interval = srsResult.NewInterval;
            progress.Repetition = srsResult.NewRepetition;
            progress.NextReviewTime = srsResult.NextReviewDate;
            progress.LastGrade = grade;
            progress.LastUpdated = _timeProvider.UtcNow;

            // Calculate retention score
            var retentionScore = _algorithm.CalculateRetentionScore(
                progress.InitialRecallCorrectCount,
                progress.InitialRecallCount - progress.InitialRecallCorrectCount,
                progress.Repetition
            );

            progress.RetentionScore = retentionScore;


            // Check if just mastered
            if (srsResult.MemoryState == "Mastered" && progress.MasteredAt == null)
            {
                progress.MasteredAt = _timeProvider.UtcNow;
                _logger.LogInformation(
                    "User {UserId} mastered vocabulary {VocabId}",
                    userId, vocabularyId);

                // TODO: Trigger achievement "Master 10 words"
            }

            progress.MemoryState = srsResult.MemoryState;

            // Set first learned date if not set
            if (progress.FirstLearnedAt == null)
            {
                progress.FirstLearnedAt = _timeProvider.UtcNow;
            }

            // 5. Save to database
            await _uow.UserVocabularyProgress.UpdateSrsParametersAsync(progress, ct);

            // 8. Return result
            return new SRSUpdateResult
            {
                Success = true,
                NewEaseFactor = srsResult.NewEaseFactor,
                NewInterval = srsResult.NewInterval,
                NextReviewDate = srsResult.NextReviewDate,
                MemoryState = srsResult.MemoryState,
                RetentionScore = retentionScore,
                Message = GetEncouragingMessage(grade, srsResult.MemoryState),

                OldEaseFactor = oldEF,
                OldInterval = oldInterval,
                OldRepetition = oldRep,
                OldNextReviewDate = oldNextReview,
            };
        }

        public async Task<List<VocabularyDueDto>> GetDueVocabulariesAsync(
            int userId,
            int limit = 20,
            CancellationToken ct = default)
        {
            var now = _timeProvider.UtcNow;

            var dueProgresses = await _uow.UserVocabularyProgress
                .GetDueVocabulariesAsync(userId, now, ct);

            // Sort by priority:
            // 1. Overdue (oldest first)
            // 2. Low retention score (struggling words)
            var sorted = dueProgresses
                .OrderBy(p => p.NextReviewTime)
                .ThenBy(p => CalculateRetentionScore(p))
                .Take(limit)
                .Select(p => new VocabularyDueDto
                {
                    VocabularyId = p.VocabularyId,
                    Word = p.Vocabulary?.Word,
                    NextReviewDate = p.NextReviewTime,
                    Repetition = p.Repetition,
                    RetentionScore = CalculateRetentionScore(p),
                    DaysOverdue = (int)(now - p.NextReviewTime).TotalDays
                })
                .ToList();

            return sorted;
        }

        public async Task<decimal> GetOverallRetentionScoreAsync(
            int userId,
            CancellationToken ct = default)
        {
            var allProgresses = await _uow.UserVocabularyProgress
                .GetAllUserVocabularyProgressByUserAsync(userId, ct);  // Need to add this method

            if (!allProgresses.Any())
                return 0;

            var scores = allProgresses
                .Select(p => _algorithm.CalculateRetentionScore(
                    p.InitialRecallCorrectCount,
                    p.InitialRecallCount - p.InitialRecallCorrectCount,
                    p.Repetition
                ))
                .ToList();

            return scores.Average();
        }

        private decimal CalculateRetentionScore(UserVocabularyProgress p)
        {
            return _algorithm.CalculateRetentionScore(
                p.InitialRecallCorrectCount,
                p.InitialRecallCount - p.InitialRecallCorrectCount,
                p.Repetition
            );
        }

        private string GetEncouragingMessage(int grade, string memoryState)
        {
            return grade switch
            {
                5 => "Perfect! You've mastered this word! 🌟",
                4 => "Excellent recall! Keep it up! 💪",
                3 => "Good job! You remembered it! 👍",
                2 => "Not bad! A bit more practice and you'll get it! 📚",
                1 => "You're making progress! Keep reviewing! 🎯",
                0 => "Don't worry! This happens. Let's review this again soon! 💙",
                _ => "Keep learning!"
            };
        }

        /// <summary>
        /// Áp dụng hiệu ứng nhẹ của kết quả phát âm lên các thông số SM-2.
        /// - Perfect ×2 liên tiếp: EF +0.05 (tối đa 4.0)
        /// - Wrong ≥3 lần trên từ đang Review/Mastered: kéo gần NextReviewTime 20%
        /// - Không bao giờ reset Repetition hay thay đổi Interval
        /// </summary>
        public async Task ApplyPronunciationEffectAsync(
            int userId,
            int vocabularyId,
            Domain.Enums.PronunciationResult result,
            CancellationToken ct = default)
        {
            var progress = await _uow.UserVocabularyProgress
                .GetUserVocabularyProgressAsync(userId, vocabularyId, ct);

            if (progress == null)
            {
                _logger.LogDebug(
                    "ApplyPronunciationEffect: no progress found for User {UserId}, Vocab {VocabId}",
                    userId, vocabularyId);
                return;
            }

            switch (result)
            {
                case Domain.Enums.PronunciationResult.Perfect:
                    progress.PronunciationPerfectStreak++;
                    // Giảm nhẹ WrongCount (tối thiểu 0)
                    progress.PronunciationWrongCount = Math.Max(0, progress.PronunciationWrongCount - 1);

                    // Bonus EF sau 2 lần Perfect liên tiếp
                    if (progress.PronunciationPerfectStreak >= 2)
                    {
                        progress.EasinessFactor = Math.Min(
                            SRSAlgorithm.MAX_EASE_FACTOR,
                            progress.EasinessFactor + 0.05
                        );
                        progress.PronunciationPerfectStreak = 0; // Reset sau khi áp dụng bonus
                        _logger.LogDebug(
                            "Pronunciation bonus EF applied for User {UserId}, Vocab {VocabId}. New EF: {EF:F2}",
                            userId, vocabularyId, progress.EasinessFactor);
                    }
                    break;

                case Domain.Enums.PronunciationResult.NearMiss:
                    progress.PronunciationPerfectStreak = 0;
                    break;

                case Domain.Enums.PronunciationResult.Wrong:
                    progress.PronunciationPerfectStreak = 0;
                    progress.PronunciationWrongCount++;

                    // Penalty nhẹ: kéo ngắn NextReviewTime 20% nếu sai >= 3 lần
                    // Chỉ áp dụng cho từ đang ở Review/Mastered (không phạt từ đang học)
                    if (progress.PronunciationWrongCount >= 3
                        && (progress.MemoryState == "Review" || progress.MemoryState == "Mastered"))
                    {
                        var now = _timeProvider.UtcNow;
                        var remaining = progress.NextReviewTime - now;
                        if (remaining.TotalDays > 1) // Chỉ áp dụng khi còn nhiều hơn 1 ngày
                        {
                            var shortfall = TimeSpan.FromDays(remaining.TotalDays * 0.2);
                            progress.NextReviewTime = progress.NextReviewTime - shortfall;
                            _logger.LogDebug(
                                "Pronunciation penalty applied for User {UserId}, Vocab {VocabId}. NextReview pulled to {Date:yyyy-MM-dd}",
                                userId, vocabularyId, progress.NextReviewTime);
                        }
                    }
                    break;
            }

            progress.LastPronunciationAt = _timeProvider.UtcNow;
            await _uow.UserVocabularyProgress.UpdateSrsParametersAsync(progress, ct);
        }
    }
}
