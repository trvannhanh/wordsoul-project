using System;
using WordSoulApi.Models.DTOs.LearningSession;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Implementations;
using WordSoulApi.Repositories.Interfaces;
using WordSoulApi.Services.Interfaces;
namespace WordSoulApi.Services.Implementations
{
    public class LearningSessionService : ILearningSessionService
    {
        private readonly ILearningSessionRepository _sessionRepo;
        private readonly IVocabularyRepository _vocabRepo;
        private readonly IUserVocabularySetRepository _userVocabularySetRepository;
        private readonly IQuizQuestionRepository _quizQuestionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserOwnedPetService _userOwnedPetService;
        private readonly ILogger<LearningSessionService> _logger;


        public LearningSessionService(ILearningSessionRepository sessionRepo, IVocabularyRepository vocabRepo, IUserVocabularySetRepository userVocabularySetRepository, ILogger<LearningSessionService> logger, IQuizQuestionRepository quizQuestionRepository, IUserRepository userRepository, IUserOwnedPetService userOwnedPetService)
        {
            _sessionRepo = sessionRepo;
            _vocabRepo = vocabRepo;
            _userVocabularySetRepository = userVocabularySetRepository;
            _logger = logger;
            _quizQuestionRepository = quizQuestionRepository;
            _userRepository = userRepository;
            _userOwnedPetService = userOwnedPetService;
        }

        // Tạo một phiên học mới cho người dùng
        public async Task<LearningSessionDto> CreateLearningSessionAsync(int userId, int setId, int wordCount = 5)
        {

            // Kiểm tra xem người dùng đã có bộ từ vựng này chưa
            var exists = await _userVocabularySetRepository.CheckUserVocabualryExist(userId, setId);
            if (!exists) // Thêm khi chưa tồn tại
            {
                _logger.LogWarning("User {UserId} doesn't owns VocabularySet {vocabSetId}", userId, setId);
                throw new InvalidOperationException("VocabularySet doesn't exists for this user");
            }
            // Lấy danh sách từ chưa học
            var vocabularies = await _vocabRepo.GetUnlearnedVocabulariesFromSetAsync(userId, setId, wordCount);

            if (!vocabularies.Any())
            {
                _logger.LogInformation("No unlearned vocabularies for user {UserId} in set {vocabSetId}", userId, setId);
                throw new InvalidOperationException("No unlearned vocabularies available in this set");
            }    
                
            // Tạo session
            var session = new LearningSession
            {
                UserId = userId,
                VocabularySetId = setId,
                StartTime = DateTime.UtcNow,
                IsCompleted = false,
                SessionVocabularies = vocabularies.Select(v => new SessionVocabulary
                {
                    VocabularyId = v.Id
                }).ToList()
            };

            // Lưu session vào database
            var savedSession = await _sessionRepo.CreateLearningSessionAsync(session);

            return new LearningSessionDto
            {
                Id = savedSession.Id,
                IsCompleted = savedSession.IsCompleted,
                VocabularyIds = savedSession.SessionVocabularies.Select(v => v.VocabularyId).ToList()
            };
        }

        public async Task<CompleteSessionResponseDto> CompleteSessionAsync(int userId, int sessionId)
        {
            // Kiểm tra quyền sở hữu session
            var session = await _sessionRepo.GetLearningSessionByIdAsync(sessionId);
            if (session == null || session.UserId != userId)
                throw new UnauthorizedAccessException("User does not have access to this session");

            // Kiểm tra session chưa hoàn thành
            if (session.IsCompleted)
                throw new InvalidOperationException("Session is already completed");

            // Kiểm tra tất cả câu hỏi đã đúng
            var vocabIds = await _vocabRepo.GetVocabularyIdsBySessionIdAsync(sessionId);
            
            foreach (var vocabId in vocabIds)
            {
                // Kiểm tra xem tất cả câu hỏi liên quan đến từ vựng đã được trả lời đúng chưa
                var allCorrect = await _quizQuestionRepository.CheckAllQuestionsCorrectAsync(userId, sessionId, vocabId);
                if (!allCorrect)
                    throw new InvalidOperationException($"Not all questions for vocabulary {vocabId} are answered correctly");
            }

            // Cập nhật trạng thái session
            session.IsCompleted = true;
            session.EndTime = DateTime.UtcNow;
            await _sessionRepo.UpdateLearningSessionAsync(session);

            // Cập nhật số session đã hoàn thành trong UserVocabularySet
            await _userVocabularySetRepository.UpdateCompletedLearningSessionAsync(userId, session.VocabularySetId, 1);

            int xpEarned = 10; // giá trị có thể cấu hình
            // Cộng XP cho người dùng
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");
            user.XP += xpEarned;
            await _userRepository.UpdateUserAsync(user);

            // Cấp phát pet nếu hoàn thành 5 session
            var rewards = await _userOwnedPetService.TryGrantPetByMilestoneAsync(userId, sessionId);

            xpEarned += rewards.bonusXp;

            return new CompleteSessionResponseDto
            {
                XpEarned = xpEarned,
                IsPetRewardGranted = rewards.alreadyOwned,
                PetId = rewards.grantedPet,
                Message = rewards.alreadyOwned ? "Session completed! You earned XP and a new Pet!" : "Session completed! You earned XP!"
            };
        }

    }
}
