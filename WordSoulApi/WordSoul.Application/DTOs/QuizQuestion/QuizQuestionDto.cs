

using WordSoul.Domain.Enums;

namespace WordSoul.Application.DTOs.QuizQuestion
{
    public class QuizQuestionDto
    {
        public int VocabularyId { get; set; }        
        public QuestionType QuestionType { get; set; }
        public string? Word { get; set; }          // đáp án đúng
        public string? Meaning { get; set; }
        public string? Pronunciation { get; set; }
        public string? PartOfSpeech { get; set; }
        public string? CEFRLevel { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }

        // Cho MultipleChoice
        public List<string>? Options { get; set; }

        // Cho Listening
        public string? PronunciationUrl { get; set; }
        public bool IsRetry { get; set; }

        /// <summary>
        /// Câu hỏi được hiển thị trên GameScreen, tuỳ loại câu hỏi:
        /// - MultipleChoice: nghĩa của từ (Meaning) → user chọn Word đúng
        /// - FillInBlank: câu ví dụ từ Description với từ bị thay bằng "___"
        /// - Null: Flashcard / Listening dùng Word và Meaning bình thường
        /// </summary>
        public string? QuestionPrompt { get; set; }
    }

}
