using FluentAssertions;
using WordSoul.Application.Learning.QuestionFlow;
using WordSoul.Domain.Enums;

namespace WordSoul.Tests.Learning.QuestionFlow;

public class ReviewQuestionFlowPolicyTests
{
    private readonly ReviewQuestionFlowPolicy _policy = new();

    [Fact]
    public void Steps_StartWithUnaidedRecallAndMarkRemediation()
    {
        _policy.Version.Should().Be(QuestionFlowVersions.Current);
        _policy.TotalStages.Should().Be(3);

        _policy.GetStep(0).Should().Be(
            new FlowStep(
                QuestionType.FillInBlank,
                QuestionPhase.InitialRecall,
                RevealsAnswer: false,
                CountsAsRecall: true,
                IsRemediation: false));
        _policy.GetStep(1).Should().Be(
            new FlowStep(
                QuestionType.Flashcard,
                QuestionPhase.Feedback,
                RevealsAnswer: true,
                CountsAsRecall: false,
                IsRemediation: true));
        _policy.GetStep(2).Should().Be(
            new FlowStep(
                QuestionType.Listening,
                QuestionPhase.CorrectiveRecall,
                RevealsAnswer: false,
                CountsAsRecall: true,
                IsRemediation: true));
    }

    [Theory]
    [InlineData(0, true, null, true)]
    [InlineData(0, false, 1, false)]
    [InlineData(1, true, 2, false)]
    [InlineData(1, false, 2, false)]
    [InlineData(2, true, null, true)]
    [InlineData(2, false, 1, false)]
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
    [InlineData(3)]
    public void InvalidStage_Throws(int stageIndex)
    {
        var getStep = () => _policy.GetStep(stageIndex);
        var evaluate = () => _policy.Evaluate(stageIndex, isCorrect: true);

        getStep.Should().Throw<InvalidOperationException>();
        evaluate.Should().Throw<InvalidOperationException>();
    }
}
