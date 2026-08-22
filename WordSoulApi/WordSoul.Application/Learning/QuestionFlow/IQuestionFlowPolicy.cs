namespace WordSoul.Application.Learning.QuestionFlow;

public interface IQuestionFlowPolicy
{
    int Version { get; }
    int TotalStages { get; }

    FlowStep GetStep(int stageIndex);

    FlowTransition Evaluate(
        int stageIndex,
        bool isCorrect);
}
