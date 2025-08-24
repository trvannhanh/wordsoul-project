using WordSoulApi.Models.DTOs.AnswerRecord;
using WordSoulApi.Models.DTOs.QuizQuestion;

namespace WordSoulApi.Services.Interfaces
{
    public interface IQuizQuestionService
    {
        // Tạo một câu hỏi trắc nghiệm mới
        Task<AdminQuizQuestionDto> CreateQuizQuestionAsync(CreateQuizQuestionDto quizQuestionDto);
        // Xóa một câu hỏi trắc nghiệm theo ID
        Task<bool> DeleteQuizQuestionAsync(int id);
        // Lấy tất cả các câu hỏi trắc nghiệm
        Task<IEnumerable<AdminQuizQuestionDto>> GetAllQuizQuestionsAsync();
        // Lấy câu hỏi trắc nghiệm theo ID
        Task<QuizQuestionDto?> GetQuizQuestionByIdAsync(int id);
        // Lấy các câu hỏi trắc nghiệm cho một phiên học cụ thể
        Task<IEnumerable<QuizQuestionDto>> GetSessionQuestionsAsync(int sessionId);
        // Nộp câu trả lời cho một câu hỏi trắc nghiệm trong phiên học
        Task<SubmitAnswerResponseDto> SubmitAnswerAsync(int userId, int sessionId, SubmitAnswerRequestDto request);
        // Cập nhật một câu hỏi trắc nghiệm
        Task<AdminQuizQuestionDto> UpdateQuizQuestionAsync(int id, AdminQuizQuestionDto quizQuestionDto);
    }
}