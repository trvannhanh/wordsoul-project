using WordSoul.Application.Common;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Application.Learning.InitialRecall;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Learning.ReviewOutcome;

public sealed class ReviewOutcomeProcessor(
    IUnitOfWork uow,
    ISRSService srsService,
    IDailyQuestService dailyQuestService,
    ITimeProvider timeProvider) : IReviewOutcomeProcessor
{
    public async Task<ReviewOutcomeResult> ProcessAsync(
        int userId,
        SessionVocabulary sessionVocabulary,
        UserVocabularyProgress progress,
        InitialRecallSnapshot snapshot,
        CancellationToken ct = default)
    {
        progress.InitialRecallCount++;
        if (snapshot.IsCorrect)
            progress.InitialRecallCorrectCount++;

        await uow.UserVocabularyProgress
            .UpdateSrsParametersAsync(progress, ct);

        await dailyQuestService.UpdateQuestProgressAsync(
            userId,
            QuestType.Accuracy,
            1,
            snapshot.IsCorrect ? 1 : 0,
            ct);

        var srsResult = await srsService.UpdateAfterReviewAsync(
            userId,
            sessionVocabulary.VocabularyId,
            snapshot.Grade,
            ct);

        var history = new VocabularyReviewHistory
        {
            UserId = userId,
            VocabularyId = sessionVocabulary.VocabularyId,
            InitialRecallAnswerRecordId = snapshot.AnswerRecord.Id,
            ReviewTime = timeProvider.UtcNow,
            IsCorrect = snapshot.IsCorrect,
            ResponseTimeSeconds = snapshot.AnswerRecord.ResponseTimeSeconds,
            HintCount = snapshot.AnswerRecord.HintCount,
            Grade = snapshot.Grade,
            GradingPolicyVersion = snapshot.GradingPolicyVersion,
            GradeReason = snapshot.GradeReason,
            EaseFactorBefore = srsResult.OldEaseFactor,
            EaseFactorAfter = srsResult.NewEaseFactor,
            IntervalBefore = srsResult.OldInterval,
            IntervalAfter = srsResult.NewInterval,
            NextReviewBefore = srsResult.OldNextReviewDate,
            NextReviewAfter = srsResult.NextReviewDate
        };

        await uow.VocabularyReviewHistory
            .CreateReviewHistoryAsync(history, ct);

        return new ReviewOutcomeResult(history, srsResult);
    }
}
