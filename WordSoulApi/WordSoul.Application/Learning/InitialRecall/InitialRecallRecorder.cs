using WordSoul.Application.Learning.QuestionFlow;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Learning.InitialRecall;

public sealed class InitialRecallRecorder(
    IInitialRecallGradingPolicy gradingPolicy) : IInitialRecallRecorder
{
    public InitialRecallSnapshot? Capture(
        LearningSession session,
        SessionVocabulary sessionVocabulary,
        AnswerRecord answerRecord,
        FlowStep flowStep)
    {
        if (session.Type != SessionType.Review
            || session.FlowVersion != QuestionFlowVersions.Current
            || flowStep.Phase != QuestionPhase.InitialRecall
            || answerRecord.StageIndexBefore != 0
            || sessionVocabulary.InitialRecallAnswerRecordId.HasValue
            || sessionVocabulary.InitialRecallAnswerRecord != null)
        {
            return null;
        }

        var grade = gradingPolicy.CalculateGrade(
            answerRecord.IsCorrect,
            answerRecord.ResponseTimeSeconds,
            answerRecord.HintCount);

        sessionVocabulary.InitialRecallAnswerRecord = answerRecord;
        sessionVocabulary.InitialRecallAt = answerRecord.CreatedAt;
        sessionVocabulary.InitialRecallCorrect = answerRecord.IsCorrect;
        sessionVocabulary.InitialRecallGrade = grade;

        return new InitialRecallSnapshot(
            answerRecord,
            answerRecord.IsCorrect,
            grade,
            answerRecord.CreatedAt);
    }

    public InitialRecallSnapshot GetRequiredSnapshot(
        SessionVocabulary sessionVocabulary)
    {
        var answerRecord = sessionVocabulary.InitialRecallAnswerRecord
            ?? throw new InvalidOperationException(
                "A versioned review must link its initial recall answer record.");
        var grade = sessionVocabulary.InitialRecallGrade
            ?? throw new InvalidOperationException(
                "A versioned review must preserve its initial recall grade.");
        var capturedAt = sessionVocabulary.InitialRecallAt
            ?? throw new InvalidOperationException(
                "A versioned review must preserve its initial recall timestamp.");

        return new InitialRecallSnapshot(
            answerRecord,
            answerRecord.IsCorrect,
            grade,
            capturedAt);
    }
}
