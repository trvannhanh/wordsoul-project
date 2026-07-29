namespace WordSoul.Application.Learning.InitialRecall;

public sealed class InitialRecallGradingPolicy : IInitialRecallGradingPolicy
{
    public int CalculateGrade(
        bool isCorrect,
        double responseTimeSeconds,
        int hintCount)
    {
        if (!isCorrect)
            return hintCount > 0 ? 1 : 2;

        if (hintCount == 0 && responseTimeSeconds <= 5)
            return 5;

        if (hintCount == 0 && responseTimeSeconds <= 10)
            return 4;

        return 3;
    }
}
