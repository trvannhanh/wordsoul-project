using WordSoul.Domain.Entities;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface IUserVocabularySetRepository
    {
        // ----------------------------- CREATE -----------------------------
        /// <summary>
        /// Thêm một bộ từ vựng vào danh sách sở hữu của người dùng (mở khóa bộ từ vựng).
        /// </summary>
        Task AddVocabularySetToUserAsync(
            UserVocabularySet userVocabularySet,
            CancellationToken cancellationToken = default);

        // ----------------------------- READ -------------------------------
        /// <summary>
        /// Lấy thông tin sở hữu bộ từ vựng của người dùng theo UserId và VocabularySetId.
        /// </summary>
        Task<UserVocabularySet?> GetUserVocabularySetAsync(
            int userId,
            int vocabularySetId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Kiểm tra xem người dùng đã sở hữu (đã mở khóa) bộ từ vựng này chưa.
        /// </summary>
        Task<bool> CheckUserHasVocabularySetAsync(
            int userId,
            int vocabId,
            CancellationToken cancellationToken = default);

        // ----------------------------- UPDATE -----------------------------
        /// <summary>
        /// Cập nhật thông tin sở hữu bộ từ vựng (ví dụ: cập nhật tiến độ học, thời gian hoàn thành...).
        /// </summary>
        Task UpdateUserVocabularySetAsync(
            UserVocabularySet userVocabularySet,
            CancellationToken cancellationToken = default);
    }
}