using WordSoul.Domain.Enums;

namespace WordSoul.Application.Learning.QuestionFlow;

public sealed class QuestionFlowResolver : IQuestionFlowResolver
{
    private readonly LegacyQuestionFlowPolicy _legacyPolicy;
    private readonly LearningQuestionFlowPolicy _learningPolicy;
    private readonly ReviewQuestionFlowPolicy _reviewPolicy;

    public QuestionFlowResolver(
        LegacyQuestionFlowPolicy legacyPolicy,
        LearningQuestionFlowPolicy learningPolicy,
        ReviewQuestionFlowPolicy reviewPolicy)
    {
        _legacyPolicy = legacyPolicy;
        _learningPolicy = learningPolicy;
        _reviewPolicy = reviewPolicy;
    }

    public IQuestionFlowPolicy Resolve(
        SessionType sessionType,
        int flowVersion)
    {
        if (flowVersion == QuestionFlowVersions.Legacy)
        {
            return _legacyPolicy;
        }

        if (flowVersion != QuestionFlowVersions.Current)
        {
            throw new InvalidOperationException(
                $"Question flow version {flowVersion} is not supported.");
        }

        return sessionType switch
        {
            SessionType.Learning => _learningPolicy,
            SessionType.Review => _reviewPolicy,
            _ => throw new InvalidOperationException(
                $"Session type {sessionType} does not have a question flow.")
        };
    }
}
