using FluentAssertions;
using WordSoul.Application.Learning.QuestionFlow;

namespace WordSoul.Tests.Learning.QuestionFlow;

[Trait("Suite", "MandatoryQuestionFlow")]
public class MandatoryQuestionFlowRegressionTests
{
    [Fact]
    public void LearningV2_PerfectPath_VisitsEveryLearningPhase()
    {
        var result = Run(
            new LearningQuestionFlowPolicy(),
            startStage: 0,
            [true, true, true, true]);

        result.IsCompleted.Should().BeTrue();
        result.VisitedPhases.Should().ContainInOrder(
            QuestionPhase.Study,
            QuestionPhase.GuidedRecall,
            QuestionPhase.Recognition,
            QuestionPhase.ProductiveRecall);
    }

    [Fact]
    public void LearningV2_WrongRecall_FallsBackAndCanRecover()
    {
        var result = Run(
            new LearningQuestionFlowPolicy(),
            startStage: 0,
            [true, false, true, true, true, true]);

        result.IsCompleted.Should().BeTrue();
        result.VisitedPhases.Should().ContainInOrder(
            QuestionPhase.Study,
            QuestionPhase.GuidedRecall,
            QuestionPhase.Study,
            QuestionPhase.GuidedRecall,
            QuestionPhase.Recognition,
            QuestionPhase.ProductiveRecall);
    }

    [Fact]
    public void ReviewV2_CorrectInitialRecall_CompletesWithoutRemediation()
    {
        var result = Run(
            new ReviewQuestionFlowPolicy(),
            startStage: 0,
            [true]);

        result.IsCompleted.Should().BeTrue();
        result.VisitedPhases.Should().Equal(QuestionPhase.InitialRecall);
    }

    [Fact]
    public void ReviewV2_FailedRecall_LoopsThroughFeedbackUntilCorrected()
    {
        var result = Run(
            new ReviewQuestionFlowPolicy(),
            startStage: 0,
            [false, true, false, true, true]);

        result.IsCompleted.Should().BeTrue();
        result.VisitedPhases.Should().ContainInOrder(
            QuestionPhase.InitialRecall,
            QuestionPhase.Feedback,
            QuestionPhase.CorrectiveRecall,
            QuestionPhase.Feedback,
            QuestionPhase.CorrectiveRecall);
    }

    [Theory]
    [InlineData(false, 2, false)]
    [InlineData(true, null, true)]
    public void LegacyV1_StageThreeResume_RemainsCompatible(
        bool isCorrect,
        int? expectedNextStage,
        bool expectedCompleted)
    {
        var transition = new LegacyQuestionFlowPolicy()
            .Evaluate(stageIndex: 3, isCorrect);

        transition.NextStageIndex.Should().Be(expectedNextStage);
        transition.IsCompleted.Should().Be(expectedCompleted);
    }

    [Fact]
    public void ReviewV2_OnlyFeedbackMayRevealTheAnswer()
    {
        var policy = new ReviewQuestionFlowPolicy();
        var steps = Enumerable.Range(0, policy.TotalStages)
            .Select(policy.GetStep)
            .ToList();

        steps.Single(step => step.RevealsAnswer).Phase
            .Should().Be(QuestionPhase.Feedback);
        steps.Where(step => step.CountsAsRecall)
            .Select(step => step.Phase)
            .Should().Equal(
                QuestionPhase.InitialRecall,
                QuestionPhase.CorrectiveRecall);
    }

    private static FlowRunResult Run(
        IQuestionFlowPolicy policy,
        int startStage,
        IReadOnlyList<bool> answers)
    {
        var stage = startStage;
        var visitedPhases = new List<QuestionPhase>();

        foreach (var answer in answers)
        {
            visitedPhases.Add(policy.GetStep(stage).Phase);
            var transition = policy.Evaluate(stage, answer);
            if (transition.IsCompleted)
                return new FlowRunResult(visitedPhases, IsCompleted: true);

            stage = transition.NextStageIndex
                ?? throw new InvalidOperationException(
                    "Incomplete transition must provide a next stage.");
        }

        return new FlowRunResult(visitedPhases, IsCompleted: false);
    }

    private sealed record FlowRunResult(
        IReadOnlyList<QuestionPhase> VisitedPhases,
        bool IsCompleted);
}
