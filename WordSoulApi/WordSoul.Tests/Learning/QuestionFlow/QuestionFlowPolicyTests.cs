using FluentAssertions;
using WordSoul.Application.Learning.QuestionFlow;
using WordSoul.Domain.Enums;

namespace WordSoul.Tests.Learning.QuestionFlow;

public class QuestionFlowPolicyTests
{
    [Fact]
    public void QuestionType_PersistedValues_RemainStable()
    {
        ((int)QuestionType.Flashcard).Should().Be(0);
        ((int)QuestionType.FillInBlank).Should().Be(1);
        ((int)QuestionType.MultipleChoice).Should().Be(2);
        ((int)QuestionType.Listening).Should().Be(3);
    }

    [Fact]
    public void Steps_ContainTheCurrentFourStageFlow()
    {
        QuestionFlowPolicy.TotalStages.Should().Be(4);
        QuestionFlowPolicy.Steps.Should().ContainInOrder(
            QuestionType.Flashcard,
            QuestionType.FillInBlank,
            QuestionType.MultipleChoice,
            QuestionType.Listening);
    }

    [Theory]
    [InlineData(0, QuestionType.Flashcard)]
    [InlineData(1, QuestionType.FillInBlank)]
    [InlineData(2, QuestionType.MultipleChoice)]
    [InlineData(3, QuestionType.Listening)]
    public void GetQuestionType_MapsEveryStage(
        int stageIndex,
        QuestionType expectedQuestionType)
    {
        QuestionFlowPolicy.GetQuestionType(stageIndex)
            .Should().Be(expectedQuestionType);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    public void GetQuestionType_StageOutsideFlow_Throws(int stageIndex)
    {
        var act = () => QuestionFlowPolicy.GetQuestionType(stageIndex);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*outside question flow*");
    }
}
