using WordSoul.Application.DTOs.Vocabulary;

namespace WordSoul.Application.Interfaces.Services
{
    /// <summary>
    /// Service giao tiếp với Gemini AI để sinh metadata từ vựng tự động.
    /// </summary>
    public interface IGeminiAiService
    {
        /// <summary>
        /// Gửi batch từ vựng lên Gemini AI, nhận về danh sách metadata JSON.
        /// Tự động xử lý rate limit bằng batch + delay.
        /// </summary>
        /// <param name="words">Danh sách từ cần sinh metadata.</param>
        /// <param name="cancellationToken">Token hủy.</param>
        /// <returns>Danh sách AiGeneratedVocabularyDto. Từ nào lỗi sẽ không có trong kết quả.</returns>
        Task<List<AiGeneratedVocabularyDto>> GenerateVocabularyMetadataAsync(
            List<string> words,
            CancellationToken cancellationToken = default);
    }
}
