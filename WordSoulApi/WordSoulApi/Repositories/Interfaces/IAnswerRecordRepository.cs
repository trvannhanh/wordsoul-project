using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface IAnswerRecordRepository
    {
        // tạo bản ghi trả lời mới
        Task<AnswerRecord> CreateAnswerRecordAsync(AnswerRecord answerRecord);
        // xóa bản ghi trả lời theo ID
        Task<bool> DeleteAnswerRecordAsync(int id);
        Task<bool> ExistsAsync(int userId, int sessionId, int questionId);

        // lấy tất cả các bản ghi trả lời
        Task<IEnumerable<AnswerRecord>> GetAllAnswerRecordsAsync();
        // lấy bản ghi trả lời theo ID, bao gồm thông tin câu hỏi liên quan
        Task<AnswerRecord?> GetAnswerRecordByIdAsync(int id);
        // lấy số lần thử trả lời của người dùng cho một câu hỏi trong một phiên học cụ thể
        Task<int> GetAttemptCountAsync(int userId, int sessionId, int questionId);
        // cập nhật bản ghi trả lời
        Task<AnswerRecord> UpdateAnswerRecordAsync(AnswerRecord answerRecord);
    }
}