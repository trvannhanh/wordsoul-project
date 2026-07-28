using WordSoul.Domain.Enums;

namespace WordSoul.Application.Learning.QuestionFlow;

public interface IQuestionFlowResolver
{
    IQuestionFlowPolicy Resolve(
        SessionType sessionType,
        int flowVersion);
}
