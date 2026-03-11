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
        /// - Top 5 chủ đề yêu thích (ThemePreferences)
        /// - Gợi ý bộ từ vựng phù hợp (RecommendedSets)
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

            // Những từ vựng sai nhiều nhất (Struggle words)
            var struggleWords = progresses
                .Where(p => p.WrongCount > 0 && p.Vocabulary != null)
                .OrderByDescending(p => p.WrongCount)
                .Take(10)
                .Select(p => new StruggleWordDto
                {
                    VocabularyId = p.VocabularyId,
                    Word = p.Vocabulary!.Word,
                    Meaning = p.Vocabulary.Meaning,
                    WrongCount = p.WrongCount
                })
                .ToList();

            // ──────────────────────────────────────────────────────────────
            // PERSONALIZATION: Sở thích chủ đề + Gợi ý bộ từ vựng
            // ──────────────────────────────────────────────────────────────

            // 1. Lấy Top 5 chủ đề yêu thích dựa trên số phiên học hoàn thành
            var favThemes = await _uow.LearningSession
                .GetUserFavoriteThemesAsync(userId, 5, cancellationToken);

            var themePreferences = favThemes
                .Select(t => new ThemePreferenceDto
                {
                    Theme = t.Theme.ToString(),
                    CompletedSessionsCount = t.Count
                })
                .ToList();

            // 2. Gợi ý bộ từ theo chủ đề yêu thích (chỉ khi đã có data)
            var recommendedSets = new List<RecommendedSetDto>();
            if (favThemes.Count > 0)
            {
                var recommended = await _uow.VocabularySet
                    .GetRecommendedSetsForUserAsync(
                        userId,
                        favThemes.Select(t => t.Theme),
                        4,
                        cancellationToken);

                recommendedSets = recommended
                    .Select(s => new RecommendedSetDto
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Theme = s.Theme.ToString(),
                        ImageUrl = s.ImageUrl,
                        Description = s.Description,
                        DifficultyLevel = s.DifficultyLevel.ToString()
                    })
                    .ToList();
            }

            var result = new UserProgressDto
            {
                ReviewWordCount = reviewWordCount,
                NextReviewTime = nextReviewTime,
                VocabularyStats = vocabularyStats,
                StruggleWords = struggleWords,
                ThemePreferences = themePreferences,
                RecommendedSets = recommendedSets
            };

            _logger.LogInformation(
                "User {UserId} progress: {ReviewCount} words to review, next review at {NextReview:u}, stats: {Stats}, top theme: {TopTheme}",
                userId,
                reviewWordCount,
                nextReviewTime,
                string.Join(", ", vocabularyStats.Select(s => $"{s.Level}:{s.Count}")),
                themePreferences.FirstOrDefault()?.Theme ?? "none");

            return result;
        }
       
    }
}