namespace WordSoul.Application.Learning.InitialRecall;

public interface IInitialRecallGradingPolicy
{
    InitialRecallGradeResult Evaluate(
        bool isCorrect,
        double responseTimeSeconds,
        int hintCount);
}
