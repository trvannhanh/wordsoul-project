namespace WordSoul.Application.Interfaces.Services
{
    /// <summary>
    /// Service lấy URL ảnh từ Unsplash API theo từ khóa.
    /// </summary>
    public interface IUnsplashService
    {
        /// <summary>
        /// Lấy URL ảnh "regular" (~1080px) đầu tiên từ Unsplash theo từ khóa.
        /// </summary>
        /// <param name="query">Từ khóa tìm kiếm (thường là từ vựng tiếng Anh).</param>
        /// <param name="cancellationToken">Token hủy.</param>
        /// <returns>URL ảnh Unsplash CDN, hoặc null nếu không tìm thấy hoặc lỗi.</returns>
        Task<string?> GetFirstImageUrlAsync(
            string query,
            CancellationToken cancellationToken = default);
    }
}
