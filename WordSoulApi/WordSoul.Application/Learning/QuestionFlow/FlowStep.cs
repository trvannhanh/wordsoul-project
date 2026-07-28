using WordSoul.Domain.Enums;

namespace WordSoul.Application.Learning.QuestionFlow;

public sealed record FlowStep(
    QuestionType QuestionType,
    QuestionPhase Phase,
    bool RevealsAnswer,
    bool CountsAsRecall,
    bool IsRemediation);
