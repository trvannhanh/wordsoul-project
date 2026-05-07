using WordSoul.Application.DTOs.VocabularySet;

namespace WordSoul.Application.Interfaces.Services
{
    /// <summary>
    /// Service cache kết quả AI sinh metadata từ vựng vào Redis.
    /// Giúp tránh gọi lại Gemini cho những từ đã được xử lý.
    /// </summary>
    public interface IVocabularyAiCacheService
    {
        /// <summary>Lấy preview DTO từ cache theo từ. Trả về null nếu cache miss.</summary>
        Task<VocabularyPreviewDto?> GetAsync(string word, CancellationToken cancellationToken = default);

        /// <summary>Lưu preview DTO vào cache.</summary>
        Task SetAsync(string word, VocabularyPreviewDto dto, CancellationToken cancellationToken = default);
    }
}
