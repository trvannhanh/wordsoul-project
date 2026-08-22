using FluentAssertions;
using WordSoul.Application.Learning.QuestionFlow;
using WordSoul.Domain.Enums;

namespace WordSoul.Tests.Learning.QuestionFlow;

public class QuestionFlowResolverTests
{
    private readonly LegacyQuestionFlowPolicy _legacyPolicy = new();
    private readonly LearningQuestionFlowPolicy _learningPolicy = new();
    private readonly ReviewQuestionFlowPolicy _reviewPolicy = new();

    [Theory]
    [InlineData(SessionType.Learning)]
    [InlineData(SessionType.Review)]
    public void Resolve_LegacyVersion_UsesLegacyPolicy(SessionType sessionType)
    {
        var resolver = CreateResolver();

        resolver.Resolve(sessionType, QuestionFlowVersions.Legacy)
            .Should().BeSameAs(_legacyPolicy);
    }

    [Fact]
    public void Resolve_CurrentLearningVersion_UsesLearningPolicy()
    {
        CreateResolver()
            .Resolve(SessionType.Learning, QuestionFlowVersions.Current)
            .Should().BeSameAs(_learningPolicy);
    }

    [Fact]
    public void Resolve_CurrentReviewVersion_UsesReviewPolicy()
    {
        CreateResolver()
            .Resolve(SessionType.Review, QuestionFlowVersions.Current)
            .Should().BeSameAs(_reviewPolicy);
    }

    [Fact]
    public void Resolve_UnsupportedVersion_Throws()
    {
        var act = () => CreateResolver()
            .Resolve(SessionType.Learning, flowVersion: 99);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*version 99*not supported*");
    }

    private QuestionFlowResolver CreateResolver() =>
        new(_legacyPolicy, _learningPolicy, _reviewPolicy);
}
