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

        var gradeResult = gradingPolicy.Evaluate(
            answerRecord.IsCorrect,
            answerRecord.ResponseTimeSeconds,
            answerRecord.HintCount);

        sessionVocabulary.InitialRecallAnswerRecord = answerRecord;
        sessionVocabulary.InitialRecallAt = answerRecord.CreatedAt;
        sessionVocabulary.InitialRecallCorrect = answerRecord.IsCorrect;
        sessionVocabulary.InitialRecallGrade = gradeResult.Grade;
        sessionVocabulary.InitialRecallGradingPolicyVersion =
            gradeResult.PolicyVersion;
        sessionVocabulary.InitialRecallGradeReason = gradeResult.Reason;

        return new InitialRecallSnapshot(
            answerRecord,
            answerRecord.IsCorrect,
            gradeResult.Grade,
            gradeResult.PolicyVersion,
            gradeResult.Reason,
            answerRecord.CreatedAt);
    }
}
