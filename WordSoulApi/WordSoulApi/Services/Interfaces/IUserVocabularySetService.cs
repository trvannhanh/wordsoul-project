namespace WordSoulApi.Services.Interfaces
{
    public interface IUserVocabularySetService
    {
        // Thêm bộ từ vựng vào người dùng
        Task AddVocabularySetToUserAsync(int userId, int vocabId);
    }
}