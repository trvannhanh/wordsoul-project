using WordSoulApi.Models.Entities;

namespace WordSoulApi.Models.DTOs.QuizQuestion
{
    public class QuizQuestionDto
    {
        public int VocabularyId { get; set; }        
        public QuestionType QuestionType { get; set; }
        public string Word { get; set; }          // đáp án đúng
        public string Meaning { get; set; }
        public string? Pronunciation { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }

        // Cho MultipleChoice
        public List<string>? Options { get; set; }

        // Cho Listening
        public string? PronunciationUrl { get; set; }
    }

}
