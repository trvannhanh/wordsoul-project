using FluentAssertions;
using WordSoul.Application.Learning.QuestionFlow;
using WordSoul.Domain.Enums;

namespace WordSoul.Tests.Learning.QuestionFlow;

public class LearningQuestionFlowPolicyTests
{
    private readonly LearningQuestionFlowPolicy _policy = new();

    [Fact]
    public void Steps_DescribeTheLearningFlow()
    {
        _policy.Version.Should().Be(QuestionFlowVersions.Current);
        _policy.TotalStages.Should().Be(4);

        _policy.GetStep(0).Should().Be(
            new FlowStep(
                QuestionType.Flashcard,
                QuestionPhase.Study,
                RevealsAnswer: true,
                CountsAsRecall: false,
                IsRemediation: false));
        _policy.GetStep(1).Phase.Should().Be(QuestionPhase.GuidedRecall);
        _policy.GetStep(2).Phase.Should().Be(QuestionPhase.Recognition);
        _policy.GetStep(3).Phase.Should().Be(QuestionPhase.ProductiveRecall);
    }

    [Theory]
    [InlineData(0, false, 0, false)]
    [InlineData(1, false, 0, false)]
    [InlineData(2, false, 1, false)]
    [InlineData(3, false, 2, false)]
    [InlineData(0, true, 1, false)]
    [InlineData(1, true, 2, false)]
    [InlineData(2, true, 3, false)]
    [InlineData(3, true, null, true)]
    public void Evaluate_ReturnsExpectedTransition(
        int currentStage,
        bool isCorrect,
        int? expectedStage,
        bool expectedCompleted)
    {
        var transition = _policy.Evaluate(currentStage, isCorrect);

        transition.NextStageIndex.Should().Be(expectedStage);
        transition.IsCompleted.Should().Be(expectedCompleted);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    public void InvalidStage_Throws(int stageIndex)
    {
        var getStep = () => _policy.GetStep(stageIndex);
        var evaluate = () => _policy.Evaluate(stageIndex, isCorrect: true);

        getStep.Should().Throw<InvalidOperationException>();
        evaluate.Should().Throw<InvalidOperationException>();
    }
}
