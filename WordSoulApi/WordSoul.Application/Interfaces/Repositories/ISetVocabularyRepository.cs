using WordSoul.Domain.Entities;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface ISetVocabularyRepository
    {
        // ----------------------------- CREATE -----------------------------
        /// <summary>
        /// Tạo một liên kết mới giữa từ vựng và bộ từ vựng.
        /// </summary>
        /// <returns>Đối tượng SetVocabulary vừa được thêm.</returns>
        Task<SetVocabulary> CreateSetVocabularyAsync(
            SetVocabulary setVocabulary,
            CancellationToken cancellationToken = default);

        // ----------------------------- READ -------------------------------
        /// <summary>
        /// Lấy thông tin liên kết giữa từ vựng và bộ từ vựng theo VocabularyId và VocabularySetId.
        /// </summary>
        Task<SetVocabulary?> GetSetVocabularyAsync(
            int vocabId,
            int setId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách từ vựng thuộc một bộ từ vựng với phân trang và tổng số bản ghi.
        /// </summary>
        Task<(IEnumerable<Vocabulary> Vocabularies, int TotalCount)> GetVocabulariesFromSetAsync(
            int vocabularySetId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy chi tiết đầy đủ của một bộ từ vựng kèm danh sách từ vựng bên trong (có phân trang).
        /// </summary>
        Task<VocabularySet?> GetVocabularySetFullDetailsAsync(
            int id,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách từ vựng chưa học của người dùng từ một bộ từ vựng cụ thể (ngẫu nhiên).
        /// </summary>
        Task<IEnumerable<Vocabulary>> GetUnlearnedVocabulariesFromSetAsync(
            int userId,
            int setId,
            int take = 5,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy các từ vựng mà người dùng cần ôn tập (theo thuật toán Spaced Repetition).
        /// </summary>
        Task<IEnumerable<Vocabulary>> GetUnreviewdVocabulariesFromSetAsync(
            int userId,
            int take = 5,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy giá trị Order lớn nhất hiện tại trong bộ từ vựng (dùng để thêm từ mới vào cuối).
        /// </summary>
        Task<int> GetVocabularyOrderMaxAsync(
            int setId,
            CancellationToken cancellationToken = default);

        // ----------------------------- DELETE -----------------------------
        /// <summary>
        /// Xóa liên kết giữa một từ vựng và một bộ từ vựng.
        /// </summary>
        /// <returns>true nếu xóa thành công, false nếu không tìm thấy liên kết.</returns>
        Task<bool> DeleteSetVocabularyAsync(
            SetVocabulary setVocabulary,
            CancellationToken cancellationToken = default);

        // ----------------------------- OTHER -----------------------------
        /// <summary>
        /// Kiểm tra xem một từ (word) đã tồn tại trong bộ từ vựng hay chưa.
        /// </summary>
        Task<bool> CheckVocabularyExistFromSetAsync(
            string word,
            int setId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Đếm tổng số từ vựng hiện có trong một bộ từ vựng.
        /// </summary>
        Task<int> CountVocabulariesInSetAsync(
            int vocabularySetId,
            CancellationToken cancellationToken = default);
    }
}