using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface IAnswerRecordRepository
    {
        Task<bool> CheckAllQuestionsCorrectAsync(int sessionId, int vocabId);
        Task<AnswerRecord> CreateAnswerRecordAsync(AnswerRecord answerRecord);
        Task<bool> DeleteAnswerRecordAsync(int id);
        Task<bool> ExistsAsync(int sessionId, int vocabId, QuestionType questionType);
        Task<IEnumerable<AnswerRecord>> GetAllAnswerRecordsAsync();
        Task<AnswerRecord?> GetAnswerRecordByIdAsync(int id);
        Task<AnswerRecord?> GetAnswerRecordFromSession(int sessionId, int vocabId, QuestionType questionType);
        Task<int> GetAttemptCountAsync(int sessionId, int vocabId, QuestionType questionType);
        Task<List<QuestionType>> GetCorrectAnswerTypesAsync(int sessionId, int vocabId);
        Task<IEnumerable<AnswerRecord>> GetWrongAnswersAsync(int sessionId);
        Task<AnswerRecord> UpdateAnswerRecordAsync(AnswerRecord answerRecord);
    }
}