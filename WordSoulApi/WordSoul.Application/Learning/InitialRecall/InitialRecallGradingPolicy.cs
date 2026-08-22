using WordSoul.Domain.Enums;

namespace WordSoul.Application.Learning.InitialRecall;

public sealed class InitialRecallGradingPolicy : IInitialRecallGradingPolicy
{
    public const int Version = 2;

    public InitialRecallGradeResult Evaluate(
        bool isCorrect,
        double responseTimeSeconds,
        int hintCount)
    {
        if (!isCorrect)
        {
            return new InitialRecallGradeResult(
                0,
                Version,
                ReviewGradeReason.FailedInitialRecall);
        }

        if (hintCount == 0 && responseTimeSeconds <= 5)
        {
            return new InitialRecallGradeResult(
                5,
                Version,
                ReviewGradeReason.FastUnaidedRecall);
        }

        if (hintCount == 0 && responseTimeSeconds <= 10)
        {
            return new InitialRecallGradeResult(
                4,
                Version,
                ReviewGradeReason.HesitantUnaidedRecall);
        }

        return new InitialRecallGradeResult(
            3,
            Version,
            ReviewGradeReason.DifficultOrAssistedRecall);
    }
}
