using WordSoul.Application.Learning.QuestionFlow;
using WordSoul.Domain.Entities;

namespace WordSoul.Application.Learning.InitialRecall;

public interface IInitialRecallRecorder
{
    InitialRecallSnapshot? Capture(
        LearningSession session,
        SessionVocabulary sessionVocabulary,
        AnswerRecord answerRecord,
        FlowStep flowStep);
}
