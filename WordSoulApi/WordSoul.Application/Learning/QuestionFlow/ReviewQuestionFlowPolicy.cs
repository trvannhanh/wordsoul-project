using WordSoul.Domain.Enums;

namespace WordSoul.Application.Learning.QuestionFlow;

public sealed class ReviewQuestionFlowPolicy : IQuestionFlowPolicy
{
    private static readonly IReadOnlyList<FlowStep> Steps =
    [
        new(QuestionType.FillInBlank, QuestionPhase.InitialRecall, false, true, false),
        new(QuestionType.Flashcard, QuestionPhase.Feedback, true, false, true),
        new(QuestionType.Listening, QuestionPhase.CorrectiveRecall, false, true, true)
    ];

    public int Version => QuestionFlowVersions.Current;
    public int TotalStages => Steps.Count;

    public FlowStep GetStep(int stageIndex)
    {
        ValidateStageIndex(stageIndex);
        return Steps[stageIndex];
    }

    public FlowTransition Evaluate(int stageIndex, bool isCorrect)
    {
        ValidateStageIndex(stageIndex);

        return stageIndex switch
        {
            0 when isCorrect => new FlowTransition(null, IsCompleted: true),
            0 => new FlowTransition(1, IsCompleted: false),
            1 => new FlowTransition(2, IsCompleted: false),
            2 when isCorrect => new FlowTransition(null, IsCompleted: true),
            2 => new FlowTransition(1, IsCompleted: false),
            _ => throw new InvalidOperationException(
                $"Stage index {stageIndex} is outside review question flow.")
        };
    }

    private void ValidateStageIndex(int stageIndex)
    {
        if (stageIndex < 0 || stageIndex >= TotalStages)
        {
            throw new InvalidOperationException(
                $"Stage index {stageIndex} is outside review question flow (0-{TotalStages - 1}).");
        }
    }
}
