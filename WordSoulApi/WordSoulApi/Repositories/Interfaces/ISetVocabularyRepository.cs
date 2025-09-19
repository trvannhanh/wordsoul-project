using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface ISetVocabularyRepository
    {

        //-------------------------------- CREATE -----------------------------------
        // Tạo mới SetVocabulary
        Task<SetVocabulary> CreateSetVocabularyAsync(SetVocabulary setVocabulary);

        //-------------------------------- READ -----------------------------------
        // Đêm số lượng từ vựng trong một bộ từ vựng cụ thể
        Task<int> CountVocabulariesInSetAsync(int vocabularySetId);
        // Lấy SetVocabulary theo vocabId và setId
        Task<SetVocabulary?> GetSetVocabularyAsync(int vocabId, int setId);
        // Lấy tất cả từ vựng chưa học từ một bộ từ vựng cụ thể của người dùng
        Task<IEnumerable<Vocabulary>> GetUnlearnedVocabulariesFromSetAsync(int userId, int setId, int take = 5);
        // Lấy tất cả từ vựng chưa ôn tập từ tất cả bộ từ vựng của người dùng
        Task<IEnumerable<Vocabulary>> GetUnreviewdVocabulariesFromSetAsync(int userId, int take = 5);
        // Lấy tất cả từ vưng từ một bộ từ vựng cụ thể với phân trang
        Task<(IEnumerable<Vocabulary> Vocabularies, int TotalCount)> GetVocabulariesFromSetAsync(int vocabularySetId, int pageNumber, int pageSize);
        // Lấy số thứ tự lớn nhất của từ vựng trong một bộ từ vựng cụ thể
        Task<int> GetVocabularyOrderMaxAsync(int setId);
        // Lấy chi tiết đầy đủ của một bộ từ vựng bao gồm các từ vựng bên trong với phân trang
        Task<VocabularySet?> GetVocabularySetFullDetailsAsync(int id, int page, int pageSize);
        //-------------------------------- DELETE -----------------------------------
        // Xóa một SetVocabulary
        Task<bool> DeleteSetVocabularyAsync(SetVocabulary setVocabulary);

        // -------------------------------- OTHER -----------------------------------
        // Kiểm tra từ vựng đã tồn tại trong bộ từ vựng cụ thể chưa
        Task<bool> CheckVocabularyExistFromSetAsync(string word, int setId);
    }
}