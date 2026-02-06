using WordSoul.Domain.Entities;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface IUserVocabularyProgressRepository
    {
        // ----------------------------- CREATE -----------------------------
        /// <summary>
        /// Tạo mới bản ghi tiến trình học từ vựng của người dùng.
        /// </summary>
        /// <returns>Đối tượng UserVocabularyProgress vừa được tạo.</returns>
        Task<UserVocabularyProgress> CreateUserVocabularyProgressAsync(
            UserVocabularyProgress progress,
            CancellationToken cancellationToken = default);

        // ----------------------------- READ -------------------------------
        /// <summary>
        /// Lấy tiến trình học từ vựng của người dùng theo UserId và VocabularyId.
        /// </summary>
        Task<UserVocabularyProgress?> GetUserVocabularyProgressAsync(
            int userId,
            int vocabularyId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách từ vựng đến hạn ôn tập của người dùng.
        /// </summary>
        Task<List<UserVocabularyProgress>> GetDueVocabulariesAsync(int userId, DateTime asOf, CancellationToken cancellationToken = default);

        // ----------------------------- UPDATE -----------------------------
        /// <summary>
        /// Cập nhật các tham số SRS của tiến trình học từ vựng.
        /// </summary>
        Task<UserVocabularyProgress> UpdateSrsParametersAsync(UserVocabularyProgress progress, CancellationToken cancellationToken = default);
    }
}