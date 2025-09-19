namespace WordSoulApi.Services.Interfaces
{
    public interface IUploadAssetsService
    {
        // Tải lên hình ảnh và trả về URL và PublicId của hình ảnh đã tải lên
        Task<(string? Url, string? PublicId)> UploadImageAsync(IFormFile file, string folder);
    }
}
