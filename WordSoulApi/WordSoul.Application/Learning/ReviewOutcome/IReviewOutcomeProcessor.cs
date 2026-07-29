using WordSoul.Application.Learning.InitialRecall;
using WordSoul.Domain.Entities;

namespace WordSoul.Application.Learning.ReviewOutcome;

public interface IReviewOutcomeProcessor
{
    Task<ReviewOutcomeResult> ProcessAsync(
        int userId,
        SessionVocabulary sessionVocabulary,
        UserVocabularyProgress progress,
        InitialRecallSnapshot snapshot,
        CancellationToken ct = default);
}
