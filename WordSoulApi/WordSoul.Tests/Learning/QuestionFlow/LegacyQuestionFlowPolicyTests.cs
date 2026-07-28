using FluentAssertions;
using WordSoul.Application.Learning.QuestionFlow;
using WordSoul.Domain.Enums;

namespace WordSoul.Tests.Learning.QuestionFlow;

public class LegacyQuestionFlowPolicyTests
{
    private readonly LegacyQuestionFlowPolicy _policy = new();

    [Fact]
    public void Steps_PreserveTheVersionOneFlow()
    {
        _policy.Version.Should().Be(QuestionFlowVersions.Legacy);
        _policy.TotalStages.Should().Be(4);
        Enumerable.Range(0, _policy.TotalStages)
            .Select(index => _policy.GetStep(index).QuestionType)
            .Should().ContainInOrder(
                QuestionType.Flashcard,
                QuestionType.FillInBlank,
                QuestionType.MultipleChoice,
                QuestionType.Listening);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(2, 1)]
    [InlineData(3, 2)]
    public void Evaluate_IncorrectAnswer_MovesBackOneStage(
        int currentStage,
        int expectedStage)
    {
        var transition = _policy.Evaluate(currentStage, isCorrect: false);

        transition.IsCompleted.Should().BeFalse();
        transition.NextStageIndex.Should().Be(expectedStage);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    [InlineData(2, 3)]
    public void Evaluate_CorrectAnswer_MovesForward(
        int currentStage,
        int expectedStage)
    {
        var transition = _policy.Evaluate(currentStage, isCorrect: true);

        transition.IsCompleted.Should().BeFalse();
        transition.NextStageIndex.Should().Be(expectedStage);
    }

    [Fact]
    public void Evaluate_CorrectFinalAnswer_CompletesFlow()
    {
        var transition = _policy.Evaluate(3, isCorrect: true);

        transition.IsCompleted.Should().BeTrue();
        transition.NextStageIndex.Should().BeNull();
    }
}
