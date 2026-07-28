namespace WordSoul.Application.Learning.QuestionFlow;

public sealed record FlowTransition(
    int? NextStageIndex,
    bool IsCompleted);
