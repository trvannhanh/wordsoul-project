using WordSoulApi.Models.DTOs.User;
using WordSoulApi.Models.DTOs.UserVocabularyProgress;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Implementations;
using WordSoulApi.Repositories.Interfaces;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Services.Implementations
{
    public class UserVocabularyProgressService : IUserVocabularyProgressService
    {
        private readonly IUserVocabularyProgressRepository _userVocabularyProgressRepository;
        private readonly ILearningSessionRepository _learningSessionRepository;
        private readonly IAnswerRecordRepository _answerRecordRepository;
        private readonly IUserRepository _userRepository;
        public UserVocabularyProgressService(IUserVocabularyProgressRepository userVocabularyProgressRepository, 
                                                ILearningSessionRepository learningSessionRepository, 
                                                IAnswerRecordRepository answerRecordRepository, IUserRepository userRepository)
        {
            _userVocabularyProgressRepository = userVocabularyProgressRepository;
            _learningSessionRepository = learningSessionRepository;
            _answerRecordRepository = answerRecordRepository;
            _userRepository = userRepository;
        }

        // Lấy tiến trình người dùng
        public async Task<UserProgressDto> GetUserProgressAsync(int userId)
        {
            var user = await _userRepository.GetUserWithRelationsAsync(userId);
            if (user == null) throw new Exception("User not found");

            var now = DateTime.UtcNow;

            // Từ cần ôn tập
            var reviewWords = user.UserVocabularyProgresses
                .Where(p => p.NextReviewTime <= now)
                .ToList();

            // Thời gian ôn tập tiếp theo
            var nextReview = user.UserVocabularyProgresses
                .Where(p => p.NextReviewTime > now)
                .OrderBy(p => p.NextReviewTime)
                .Select(p => p.NextReviewTime)
                .FirstOrDefault();

            // Thống kê theo proficiency level
            var stats = user.UserVocabularyProgresses
                .GroupBy(p => p.ProficiencyLevel)
                .Select(g => new LevelStatDto
                {
                    Level = g.Key,
                    Count = g.Count()
                }).ToList();

            return new UserProgressDto
            {
                ReviewWordCount = reviewWords.Count,
                NextReviewTime = nextReview,
                VocabularyStats = stats
            };
        }

    }
}
