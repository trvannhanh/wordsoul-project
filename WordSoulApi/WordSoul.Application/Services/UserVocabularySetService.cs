using Microsoft.Extensions.Logging;
using WordSoul.Application.DTOs.User;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;

namespace WordSoul.Application.Services
{
    public class UserVocabularySetService : IUserVocabularySetService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<UserVocabularySetService> _logger;

        public UserVocabularySetService(
            IUnitOfWork uow,
            ILogger<UserVocabularySetService> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        /// <summary>
        /// Thêm một bộ từ vựng vào danh sách học tập của người dùng.
        /// Kiểm tra quyền truy cập (public hoặc là chủ sở hữu nếu private).
        /// </summary>
        /// <param name="userId">ID người dùng.</param>
        /// <param name="vocabSetId">ID bộ từ vựng.</param>
        /// <param name="cancellationToken">Token hủy thao tác.</param>
        /// <exception cref="KeyNotFoundException">Khi user hoặc bộ từ vựng không tồn tại.</exception>
        /// <exception cref="InvalidOperationException">Khi không có quyền hoặc đã sở hữu bộ này rồi.</exception>
        public async Task AddVocabularySetToUserAsync(
            int userId,
            int vocabSetId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("User {UserId} is adding VocabularySet {SetId} to their library", userId, vocabSetId);

            // Kiểm tra user tồn tại
            var user = await _uow.User.GetUserByIdAsync(userId, cancellationToken)
                ?? throw new KeyNotFoundException($"User with ID {userId} not found.");

            // Kiểm tra bộ từ vựng tồn tại
            var vocabularySet = await _uow.VocabularySet.GetVocabularySetByIdAsync(vocabSetId, cancellationToken)
                ?? throw new KeyNotFoundException($"VocabularySet with ID {vocabSetId} not found.");

            // Kiểm tra quyền truy cập: phải là public hoặc do chính user tạo
            if (!vocabularySet.IsPublic && vocabularySet.CreatedById != userId)
            {
                _logger.LogWarning(
                    "User {UserId} attempted to access private VocabularySet {SetId} (owner: {OwnerId})",
                    userId, vocabSetId, vocabularySet.CreatedById);

                throw new InvalidOperationException("This vocabulary set is private and you are not the owner.");
            }

            // Kiểm tra đã sở hữu chưa
            var alreadyOwned = await _uow.UserVocabularySet.CheckUserHasVocabularySetAsync(userId, vocabSetId, cancellationToken);
            if (alreadyOwned)
            {
                _logger.LogInformation("User {UserId} already owns VocabularySet {SetId}", userId, vocabSetId);
                throw new InvalidOperationException("You already have this vocabulary set in your library.");
            }

            // Tạo liên kết
            var relation = new UserVocabularySet
            {
                UserId = userId,
                VocabularySetId = vocabSetId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                TotalCompletedSession = 0,
                IsCompleted = false
            };

            await _uow.UserVocabularySet.AddVocabularySetToUserAsync(relation, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully added VocabularySet {SetId} to User {UserId}", vocabSetId, userId);
        }

        /// <summary>
        /// Lấy thông tin chi tiết về mối quan hệ giữa người dùng và một bộ từ vựng cụ thể
        /// (số lần hoàn thành, trạng thái hoàn thành, v.v.).
        /// </summary>
        /// <param name="userId">ID người dùng.</param>
        /// <param name="vocabSetId">ID bộ từ vựng.</param>
        /// <param name="cancellationToken">Token hủy thao tác.</param>
        /// <returns>DTO chứa thông tin tiến trình của người dùng với bộ từ vựng.</returns>
        /// <exception cref="KeyNotFoundException">Khi người dùng không sở hữu bộ từ vựng này.</exception>
        public async Task<UserVocabularySetDto> GetUserVocabularySetAsync(
            int userId,
            int vocabSetId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching UserVocabularySet info for User {UserId} - Set {SetId}", userId, vocabSetId);

            var relation = await _uow.UserVocabularySet.GetUserVocabularySetAsync(userId, vocabSetId, cancellationToken);

            if (relation == null)
            {
                _logger.LogWarning("User {UserId} does not own VocabularySet {SetId}", userId, vocabSetId);
                throw new KeyNotFoundException($"User {userId} does not have VocabularySet {vocabSetId} in their library.");
            }

            return new UserVocabularySetDto
            {
                VocabularySetId = relation.VocabularySetId,
                TotalCompletedSession = relation.TotalCompletedSession,
                IsCompleted = relation.IsCompleted,
                IsActive = relation.IsActive,
                CreatedAt = relation.CreatedAt
            };
        }
    }
}