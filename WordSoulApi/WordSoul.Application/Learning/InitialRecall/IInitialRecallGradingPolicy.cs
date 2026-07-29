namespace WordSoul.Application.Learning.InitialRecall;

public interface IInitialRecallGradingPolicy
{
    int CalculateGrade(
        bool isCorrect,
        double responseTimeSeconds,
        int hintCount);
}
