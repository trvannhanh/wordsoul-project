using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface IAnswerRecordRepository
    {
        //-------------------------------- CREATE -----------------------------------
        // Tạo mới AnswerRecord
        Task<AnswerRecord> CreateAnswerRecordAsync(AnswerRecord answerRecord);
        //------------------------------- READ -----------------------------------
        // Lấy tất cả AnswerRecord
        Task<AnswerRecord?> GetAnswerRecordByIdAsync(int id);
        // Lấy tất cả AnswerRecord cho một phiên học và từ vựng cụ thể
        Task<AnswerRecord?> GetAnswerRecordFromSession(int sessionId, int vocabId, QuestionType questionType);
        // Lấy tất cả AnswerRecord cho một phiên học và từ vựng cụ thể
        Task<int> GetAttemptCountAsync(int sessionId, int vocabId, QuestionType questionType);
        //-------------------------------- UPDATE -----------------------------------
        // Cập nhật AnswerRecord
        Task<AnswerRecord> UpdateAnswerRecordAsync(AnswerRecord answerRecord);
    }
}