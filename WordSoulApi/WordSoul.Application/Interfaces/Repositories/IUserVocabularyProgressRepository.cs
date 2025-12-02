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

        // ----------------------------- UPDATE -----------------------------
        /// <summary>
        /// Cập nhật tiến trình học từ vựng (ví dụ: tăng Strength, cập nhật NextReviewTime, EFactor...).
        /// </summary>
        /// <returns>Đối tượng UserVocabularyProgress sau khi cập nhật.</returns>
        Task<UserVocabularyProgress> UpdateUserVocabularyProgressAsync(
            UserVocabularyProgress progress,
            CancellationToken cancellationToken = default);
    }
}