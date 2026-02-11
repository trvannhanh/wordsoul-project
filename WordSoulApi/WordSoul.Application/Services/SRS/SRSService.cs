using Microsoft.Extensions.Logging;
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

        public SRSService(
            IUnitOfWork uow,
            SRSAlgorithm algorithm,
            ILogger<SRSService> logger)
        {
            _uow = uow;
            _algorithm = algorithm;
            _logger = logger;
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
                progress.Repetition
            );

            // 4. Update progress entity
            progress.EasinessFactor = srsResult.NewEaseFactor;
            progress.Interval = srsResult.NewInterval;
            progress.Repetition = srsResult.NewRepetition;
            progress.NextReviewTime = srsResult.NextReviewDate;
            progress.LastGrade = grade;
            progress.LastUpdated = DateTime.UtcNow;


            // Update counts
            if (grade >= 3)
            {
                progress.CorrectAttempt++;
            }
            else
            {
                // Consider adding WrongCount field
            }
            progress.TotalAttempt++;

            // Calculate retention score
            var retentionScore = _algorithm.CalculateRetentionScore(
                progress.CorrectAttempt,
                progress.TotalAttempt - progress.CorrectAttempt,
                progress.Repetition
            );

            progress.RetentionScore = retentionScore;


            // Check if just mastered
            if (srsResult.MemoryState == "Mastered" && progress.MasteredAt == null)
            {
                progress.MasteredAt = DateTime.UtcNow;
                _logger.LogInformation(
                    "User {UserId} mastered vocabulary {VocabId}",
                    userId, vocabularyId);

                // TODO: Trigger achievement "Master 10 words"
            }

            progress.MemoryState = srsResult.MemoryState;

            // Set first learned date if not set
            if (progress.FirstLearnedAt == null)
            {
                progress.FirstLearnedAt = DateTime.UtcNow;
            }

            // 5. Save to database
            await _uow.UserVocabularyProgress.UpdateSrsParametersAsync(progress, ct);


            // 7. Commit transaction
            await _uow.SaveChangesAsync(ct);

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
            var now = DateTime.UtcNow;

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
                    p.CorrectAttempt,
                    p.TotalAttempt - p.CorrectAttempt,
                    p.Repetition
                ))
                .ToList();

            return scores.Average();
        }

        private decimal CalculateRetentionScore(UserVocabularyProgress p)
        {
            return _algorithm.CalculateRetentionScore(
                p.CorrectAttempt,
                p.TotalAttempt - p.CorrectAttempt,
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
    }
}
