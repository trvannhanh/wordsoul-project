namespace WordSoul.Application.Interfaces.Services
{
    /// <summary>
    /// Service dùng Azure AI Speech SDK để tổng hợp giọng nói và upload lên Azure Blob Storage.
    /// </summary>
    public interface IAzureSpeechService
    {
        /// <summary>
        /// Tổng hợp text thành audio MP3 và upload lên Azure Blob Storage.
        /// </summary>
        /// <param name="text">Văn bản cần tổng hợp.</param>
        /// <param name="blobName">Tên file trong Azure Blob (ví dụ: "apple-word.mp3", "apple-example.mp3").</param>
        /// <param name="cancellationToken">Token hủy.</param>
        /// <returns>Public URL của file audio trong Azure Blob, hoặc null nếu lỗi.</returns>
        Task<string?> SynthesizeAndUploadAsync(
            string text,
            string blobName,
            CancellationToken cancellationToken = default);
    }
}
