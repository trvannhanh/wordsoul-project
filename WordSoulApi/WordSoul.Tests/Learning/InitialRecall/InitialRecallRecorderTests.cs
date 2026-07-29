using FluentAssertions;
using WordSoul.Application.Learning.InitialRecall;
using WordSoul.Application.Learning.QuestionFlow;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Tests.Learning.InitialRecall;

[Trait("Suite", "MandatoryQuestionFlow")]
public class InitialRecallRecorderTests
{
    private readonly InitialRecallRecorder _recorder =
        new(new InitialRecallGradingPolicy());

    [Theory]
    [InlineData(true, 2, 0, 5)]
    [InlineData(true, 8, 0, 4)]
    [InlineData(true, 12, 0, 3)]
    [InlineData(true, 2, 1, 3)]
    [InlineData(false, 2, 0, 2)]
    [InlineData(false, 2, 1, 1)]
    public void Capture_ReviewV2InitialRecall_PersistsRawSignalAndGrade(
        bool isCorrect,
        double responseTimeSeconds,
        int hintCount,
        int expectedGrade)
    {
        var session = CreateSession(SessionType.Review, QuestionFlowVersions.Current);
        var sessionVocabulary = new SessionVocabulary();
        var answer = CreateAnswer(isCorrect, responseTimeSeconds, hintCount);

        var snapshot = _recorder.Capture(
            session,
            sessionVocabulary,
            answer,
            InitialRecallStep());

        snapshot.Should().NotBeNull();
        sessionVocabulary.InitialRecallAnswerRecord.Should().BeSameAs(answer);
        sessionVocabulary.InitialRecallAt.Should().Be(answer.CreatedAt);
        sessionVocabulary.InitialRecallCorrect.Should().Be(isCorrect);
        sessionVocabulary.InitialRecallGrade.Should().Be(expectedGrade);
    }

    [Theory]
    [InlineData(SessionType.Learning, QuestionFlowVersions.Current)]
    [InlineData(SessionType.Review, QuestionFlowVersions.Legacy)]
    public void Capture_NonVersionedReview_DoesNotCreateSnapshot(
        SessionType sessionType,
        int flowVersion)
    {
        var sessionVocabulary = new SessionVocabulary();

        var snapshot = _recorder.Capture(
            CreateSession(sessionType, flowVersion),
            sessionVocabulary,
            CreateAnswer(true, 2, 0),
            InitialRecallStep());

        snapshot.Should().BeNull();
        sessionVocabulary.InitialRecallAnswerRecord.Should().BeNull();
        sessionVocabulary.InitialRecallGrade.Should().BeNull();
    }

    [Fact]
    public void Capture_WhenSnapshotAlreadyExists_IsImmutable()
    {
        var firstAnswer = CreateAnswer(false, 7, 0);
        var sessionVocabulary = new SessionVocabulary();
        var session = CreateSession(SessionType.Review, QuestionFlowVersions.Current);

        _recorder.Capture(session, sessionVocabulary, firstAnswer, InitialRecallStep());
        var secondCapture = _recorder.Capture(
            session,
            sessionVocabulary,
            CreateAnswer(true, 1, 0),
            InitialRecallStep());

        secondCapture.Should().BeNull();
        sessionVocabulary.InitialRecallAnswerRecord.Should().BeSameAs(firstAnswer);
        sessionVocabulary.InitialRecallCorrect.Should().BeFalse();
        sessionVocabulary.InitialRecallGrade.Should().Be(2);
    }

    private static LearningSession CreateSession(
        SessionType type,
        int flowVersion) => new()
    {
        Type = type,
        FlowVersion = flowVersion
    };

    private static AnswerRecord CreateAnswer(
        bool isCorrect,
        double responseTimeSeconds,
        int hintCount) => new()
    {
        SubmissionId = Guid.NewGuid(),
        Answer = "answer",
        QuestionType = QuestionType.FillInBlank,
        QuestionPhase = QuestionPhase.InitialRecall,
        FlowVersion = QuestionFlowVersions.Current,
        StageIndexBefore = 0,
        IsCorrect = isCorrect,
        ResponseTimeSeconds = responseTimeSeconds,
        HintCount = hintCount,
        CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    };

    private static FlowStep InitialRecallStep() => new(
        QuestionType.FillInBlank,
        QuestionPhase.InitialRecall,
        RevealsAnswer: false,
        CountsAsRecall: true,
        IsRemediation: false);
}
