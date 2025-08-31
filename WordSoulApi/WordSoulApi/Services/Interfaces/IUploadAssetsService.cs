namespace WordSoulApi.Services.Interfaces
{
    public interface IUploadAssetsService
    {
        Task<(string? Url, string? PublicId)> UploadImageAsync(IFormFile file, string folder);
    }
}
