using Microsoft.Extensions.Logging;
using WordSoul.Application.DTOs.User;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Services;

namespace WordSoul.Application.Services
{
    public class UserVocabularyProgressService : IUserVocabularyProgressService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<UserVocabularyProgressService> _logger;

        public UserVocabularyProgressService(
            IUnitOfWork uow,
            ILogger<UserVocabularyProgressService> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        /// <summary>
        /// Lấy thông tin tiến trình học từ vựng của người dùng:
        /// - Số từ cần ôn tập ngay hôm nay
        /// - Thời gian ôn tập tiếp theo gần nhất
        /// - Thống kê số lượng từ theo mức độ thành thạo (ProficiencyLevel)
        /// </summary>
        /// <param name="userId">ID của người dùng.</param>
        /// <param name="cancellationToken">Token để hủy thao tác bất đồng bộ.</param>
        /// <returns>DTO chứa thông tin tiến trình học từ vựng.</returns>
        /// <exception cref="KeyNotFoundException">Khi người dùng không tồn tại.</exception>
        public async Task<UserProgressDto> GetUserProgressAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching vocabulary learning progress for User ID {UserId}", userId);

            var user = await _uow.User.GetUserWithRelationsAsync(userId, cancellationToken)
                ?? throw new KeyNotFoundException($"User with ID {userId} not found.");

            var progresses = user.UserVocabularyProgresses;

            if (progresses == null || !progresses.Any())
            {
                _logger.LogDebug("User {UserId} has no vocabulary progress records yet.", userId);
                return new UserProgressDto
                {
                    ReviewWordCount = 0,
                    NextReviewTime = null,
                    VocabularyStats = new List<LevelStatDto>()
                };
            }

            var now = DateTime.UtcNow;

            // Từ cần ôn tập ngay (NextReviewTime <= hiện tại)
            int reviewWordCount = progresses.Count(p => p.NextReviewTime <= now);

            // Thời gian ôn tập tiếp theo gần nhất
            DateTime? nextReviewTime = progresses
                .Where(p => p.NextReviewTime > now)
                .OrderBy(p => p.NextReviewTime)
                .Select(p => p.NextReviewTime)
                .FirstOrDefault();

            // Thống kê số từ theo từng mức độ thành thạo
            var vocabularyStats = progresses
                .GroupBy(p => p.ProficiencyLevel)
                .Select(g => new LevelStatDto
                {
                    Level = g.Key,
                    Count = g.Count()
                })
                .OrderBy(s => s.Level)
                .ToList();

            var result = new UserProgressDto
            {
                ReviewWordCount = reviewWordCount,
                NextReviewTime = nextReviewTime,
                VocabularyStats = vocabularyStats
            };

            _logger.LogInformation(
                "User {UserId} progress: {ReviewCount} words to review, next review at {NextReview:u}, stats: {Stats}",
                userId,
                reviewWordCount,
                nextReviewTime,
                string.Join(", ", vocabularyStats.Select(s => $"{s.Level}:{s.Count}")));

            return result;
        }

        public async Task UpdateProgressAfterReviewAsync(
            int userId,
            int vocabularyId,
            int grade,
            CancellationToken cancellationToken = default)
        {
            grade = NormalizeGrade(grade);

            var progress = await _uow.UserVocabularyProgress
                .GetUserVocabularyProgressAsync(userId, vocabularyId);

            if (progress == null) return;

            if (grade >= 3)
            {
                progress.Interval = CalculateNextInterval(
                    progress.Repetition,
                    progress.Interval,
                    progress.EasinessFactor);

                progress.Repetition++;
            }
            else
            {
                progress.Repetition = 0;
                progress.Interval = 1;
            }

            progress.EasinessFactor =
                UpdateEasinessFactor(progress.EasinessFactor, grade);
            progress.LastGrade = grade;
            progress.LastUpdated = DateTime.UtcNow;
            progress.NextReviewTime =
                DateTime.UtcNow.AddDays(progress.Interval);

            await _uow.UserVocabularyProgress.UpdateSrsParametersAsync(progress);
            await _uow.SaveChangesAsync();
        }


        private static int NormalizeGrade(int grade)
        {
            return Math.Clamp(grade, 0, 5);
        }

        private static int CalculateNextInterval(
            int repetition,
            int currentInterval,
            double easinessFactor)
        {
            // SM-2 original logic:
            // n=1 -> 1 day
            // n=2 -> 6 days
            // n>=3 -> previous interval * EF
            return repetition switch
            {
                0 => 1,
                1 => 6,
                _ => (int)Math.Round(currentInterval * easinessFactor)
            };
        }

        private static double UpdateEasinessFactor(double ef, int grade)
        {
            var newEf = ef + (0.1 - (5 - grade) * (0.08 + (5 - grade) * 0.02));
            return Math.Max(1.3, newEf);
        }
    }
}