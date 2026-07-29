namespace WordSoul.Domain.Enums;

// Values are persisted in AnswerRecord. Never reorder or reuse them.
public enum QuestionPhase
{
    Study = 0,
    GuidedRecall = 1,
    Recognition = 2,
    ProductiveRecall = 3,
    InitialRecall = 4,
    Feedback = 5,
    CorrectiveRecall = 6
}
