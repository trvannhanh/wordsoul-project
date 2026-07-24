using WordSoul.Domain.Enums;

namespace WordSoul.Application.Learning.QuestionFlow;

/// <summary>
/// Defines the single question progression shared by learning and review sessions.
/// Keep this policy simple until the product needs different or versioned flows.
/// </summary>
public static class QuestionFlowPolicy
{
    public static IReadOnlyList<QuestionType> Steps { get; } = Array.AsReadOnly(
    [
        QuestionType.Flashcard,
        QuestionType.FillInBlank,
        QuestionType.MultipleChoice,
        QuestionType.Listening
    ]);

    public static int TotalStages => Steps.Count;

    public static QuestionType GetQuestionType(int stageIndex)
    {
        if (stageIndex < 0 || stageIndex >= TotalStages)
        {
            throw new InvalidOperationException(
                $"Stage index {stageIndex} is outside question flow (0-{TotalStages - 1}).");
        }

        return Steps[stageIndex];
    }
}
