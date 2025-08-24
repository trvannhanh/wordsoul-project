using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface IQuizQuestionRepository
    {
        // Kiểm tra tất cả câu hỏi đã trả lời đúng trong một phiên học cho một từ vựng cụ thể
        Task<bool> CheckAllQuestionsCorrectAsync(int userId, int sessionId, int vocabId);
        // Tạo câu hỏi quiz mới
        Task<QuizQuestion> CreateQuizQuestionAsync(QuizQuestion quizQuestion);
        //Xóa câu hỏi quiz theo ID
        Task<bool> DeleteQuizQuestionAsync(int id);
        // Lấy tất cả các câu hỏi quiz
        Task<IEnumerable<QuizQuestion>> GetAllQuizQuestionsAsync();
        // Lấy các câu hỏi quiz theo danh sách ID từ vựng
        Task<IEnumerable<QuizQuestion>> GetQuestionsByVocabularyIdsAsync(IEnumerable<int> vocabularyIds);
        // Lấy danh sách ID câu hỏi quiz theo ID từ vựng
        Task<List<int>> GetQuizIdsByVocabularyAsync(int vocabId);
        // Lấy câu hỏi quiz theo ID, bao gồm thông tin từ vựng liên quan
        Task<QuizQuestion?> GetQuizQuestionByIdAsync(int id);
        // Cập nhật câu hỏi quiz
        Task<QuizQuestion> UpdateQuizQuestionAsync(QuizQuestion quizQuestion);
    }
}