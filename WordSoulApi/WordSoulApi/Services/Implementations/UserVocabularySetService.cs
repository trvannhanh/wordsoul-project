using Microsoft.EntityFrameworkCore;
using WordSoulApi.Models.DTOs.User;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Implementations;
using WordSoulApi.Repositories.Interfaces;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Services.Implementations
{
    public class UserVocabularySetService : IUserVocabularySetService
    {
        private readonly IUserVocabularySetRepository _userVocabularySetRepository;
        private readonly IVocabularySetRepository _vocabularySetRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserVocabularySetService> _logger;


        public UserVocabularySetService(IUserVocabularySetRepository userVocabularySetRepository, IVocabularySetRepository vocabularySetRepository, ILogger<UserVocabularySetService> logger, IUserRepository userRepository)
        {
            _userVocabularySetRepository = userVocabularySetRepository;
            _vocabularySetRepository = vocabularySetRepository;
            _logger = logger;
            _userRepository = userRepository;
        }

        // Thêm bộ từ vựng vào người dùng
        public async Task AddVocabularySetToUserAsync(int userId, int vocabSetId)
        {
            // Kiểm tra user tồn tại
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                _logger.LogError("User with ID: {UserId} not found", userId);
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            // Kiểm tra xem bộ từ vựng có tồn tại không
            var vocabularySet = await _vocabularySetRepository.GetVocabularySetByIdAsync(vocabSetId);
            if (vocabularySet == null)
            {
                _logger.LogError("VocabularySet with ID: {VocabSetId} not found", vocabSetId);
                throw new KeyNotFoundException($"VocabularySet with ID {vocabSetId} not found.");
            }

            // Kiểm tra quyền truy cập
            if (!vocabularySet.IsPublic && vocabularySet.CreatedById != userId)
            {
                _logger.LogWarning("User {UserId} attempted to access private VocabularySet {VocabSetId}", userId, vocabSetId);
                throw new InvalidOperationException("This vocabulary set is private and you are not the owner.");
            }

            // Kiểm tra xem người dùng đã có bộ từ vựng này chưa
            var exists = await _userVocabularySetRepository.CheckUserVocabularyExist(userId, vocabSetId); // Sửa typo
            if (exists)
            {
                _logger.LogWarning("User {UserId} already owns VocabularySet {VocabSetId}", userId, vocabSetId);
                throw new InvalidOperationException("You already own this vocabulary set.");
            }

            // Thêm UserVocabularySet
            var relation = new UserVocabularySet
            {
                UserId = userId,
                VocabularySetId = vocabSetId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            await _userVocabularySetRepository.AddVocabularySetToUserAsync(relation);
        }

        public async Task<UserVocabularySetDto> GetUserVocabularySetAsync(int userId, int vocabSetId)
        {
            var exists = await _userVocabularySetRepository.CheckUserVocabularyExist(userId, vocabSetId);
            if (!exists)
            {
                _logger.LogError("User {UserId} does not own VocabularySet {vocabSetId}", userId, vocabSetId);
                throw new KeyNotFoundException($"User {userId} does not own VocabularySet {vocabSetId}");
            }
            var userVocabSet = await _userVocabularySetRepository.GetUserVocabularySetAsync(userId, vocabSetId);
            if (userVocabSet == null)
            {
                _logger.LogError("Failed to retrieve UserVocabularySet for User {UserId} and VocabularySet {vocabSetId}", userId, vocabSetId);
                throw new Exception("Failed to retrieve UserVocabularySet");
            }
            return new UserVocabularySetDto { 
                VocabularySetId = userVocabSet.VocabularySetId,
                TotalCompletedSession = userVocabSet.totalCompletedSession,
                IsCompleted = userVocabSet.isCompleted,
                IsActive = userVocabSet.IsActive,
                CreatedAt = userVocabSet.CreatedAt

            };
        }
    }
}
