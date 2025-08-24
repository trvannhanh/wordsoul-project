using Microsoft.EntityFrameworkCore;
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
        private readonly ILogger<UserVocabularySetService> _logger;


        public UserVocabularySetService(IUserVocabularySetRepository userVocabularySetRepository, IVocabularySetRepository vocabularySetRepository, ILogger<UserVocabularySetService> logger)
        {
            _userVocabularySetRepository = userVocabularySetRepository;
            _vocabularySetRepository = vocabularySetRepository;
            _logger = logger;
        }

        // Thêm bộ từ vựng vào người dùng
        public async Task AddVocabularySetToUserAsync(int userId, int vocabSetId)
        {
            // Kiểm tra xem bộ từ vựng có tồn tại không
            var vocabularySet = await _vocabularySetRepository.GetVocabularySetByIdAsync(vocabSetId);
            if (vocabularySet == null)
            {
                _logger.LogError("Vocabulary with ID: {vocabSetId} not found", vocabSetId);
                throw new KeyNotFoundException($"VocabularySet with ID {vocabSetId} not found.");
            }

            // Kiểm tra xem người dùng đã có bộ từ vựng này chưa
            var exists = await _userVocabularySetRepository.CheckUserVocabualryExist(userId, vocabSetId);
            if (!exists) // Thêm khi chưa tồn tại
            {
                var relation = new UserVocabularySet { UserId = userId, VocabularySetId = vocabSetId };
                await _userVocabularySetRepository.AddVocabularySetToUserAsync(relation);
            }
            else
            {
                _logger.LogWarning("User {UserId} already owns VocabularySet {vocabSetId}", userId, vocabSetId);
                throw new InvalidOperationException("VocabularySet already exists for this user");
            }
        }
    }
}
