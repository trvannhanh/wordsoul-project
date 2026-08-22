

namespace WordSoul.Domain.Enums
{
    public enum QuestionType
    {
        // Values are persisted in AnswerRecord. Never reorder or reuse them.
        Flashcard = 0,
        FillInBlank = 1,
        MultipleChoice = 2,
        Listening = 3
    }
}
