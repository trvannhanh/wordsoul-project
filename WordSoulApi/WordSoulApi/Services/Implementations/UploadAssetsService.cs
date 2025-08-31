using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Services.Implementations
{
    public class UploadAssetsService : IUploadAssetsService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<UploadAssetsService> _logger;  

        public UploadAssetsService(Cloudinary cloudinary, ILogger<UploadAssetsService> logger)
        {
            _cloudinary = cloudinary;
            _logger = logger;
        }

        // Phương thức tải lên tệp và trả về URL của tệp đã tải lên
        public async Task<(string? Url, string? PublicId)> UploadImageAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("No file provided for upload.");
                return (null, null);
            }

            try
            {
                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = folder,
                    UseFilename = true,
                    UniqueFilename = true,
                    Overwrite = false
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    _logger.LogError("Image upload failed: {Error}", uploadResult.Error?.Message);
                    throw new Exception($"Image upload failed: {uploadResult.Error?.Message}");
                }

                _logger.LogInformation("Image uploaded successfully: {Url}", uploadResult.SecureUrl);
                return (uploadResult.SecureUrl.ToString(), uploadResult.PublicId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image to Cloudinary.");
                throw;
            }
        }
    }
}
