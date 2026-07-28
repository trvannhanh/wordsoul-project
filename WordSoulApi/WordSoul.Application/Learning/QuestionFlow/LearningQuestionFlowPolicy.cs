using WordSoul.Domain.Enums;

namespace WordSoul.Application.Learning.QuestionFlow;

public sealed class LearningQuestionFlowPolicy : IQuestionFlowPolicy
{
    private static readonly IReadOnlyList<FlowStep> Steps =
    [
        new(QuestionType.Flashcard, QuestionPhase.Study, true, false, false),
        new(QuestionType.FillInBlank, QuestionPhase.GuidedRecall, false, true, false),
        new(QuestionType.MultipleChoice, QuestionPhase.Recognition, false, true, false),
        new(QuestionType.Listening, QuestionPhase.ProductiveRecall, false, true, false)
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

        if (!isCorrect)
        {
            return new FlowTransition(
                Math.Max(0, stageIndex - 1),
                IsCompleted: false);
        }

        var nextStageIndex = stageIndex + 1;
        return nextStageIndex >= TotalStages
            ? new FlowTransition(null, IsCompleted: true)
            : new FlowTransition(nextStageIndex, IsCompleted: false);
    }

    private void ValidateStageIndex(int stageIndex)
    {
        if (stageIndex < 0 || stageIndex >= TotalStages)
        {
            throw new InvalidOperationException(
                $"Stage index {stageIndex} is outside learning question flow (0-{TotalStages - 1}).");
        }
    }
}
