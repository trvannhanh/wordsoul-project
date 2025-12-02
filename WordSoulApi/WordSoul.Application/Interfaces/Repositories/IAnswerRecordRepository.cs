using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface IAnswerRecordRepository
    {
        // ----------------------------- CREATE -----------------------------
        Task<AnswerRecord> CreateAnswerRecordAsync(
            AnswerRecord answerRecord,
            CancellationToken cancellationToken = default);

        // ----------------------------- READ -------------------------------
        Task<AnswerRecord?> GetAnswerRecordByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<AnswerRecord?> GetAnswerRecordFromSession(
            int sessionId,
            int vocabId,
            QuestionType questionType,
            CancellationToken cancellationToken = default);

        Task<int> GetAttemptCountAsync(
            int sessionId,
            int vocabId,
            QuestionType questionType,
            CancellationToken cancellationToken = default);

        Task<int> GetCorrectAnswerRecordNumberFromSession(
            int sessionId,
            CancellationToken cancellationToken = default);

        // ----------------------------- UPDATE -----------------------------
        Task<AnswerRecord> UpdateAnswerRecordAsync(
            AnswerRecord answerRecord,
            CancellationToken cancellationToken = default);

        // ----------------------------- DELETE / OTHER ---------------------
        // (Không có phương thức Delete trong implementation hiện tại)
    }
}