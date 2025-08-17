using System.ComponentModel.DataAnnotations;
using WordSoulApi.Models.Entities;

namespace WordSoulApi.Models.DTOs.QuizQuestion
{
    public class CreateQuizQuestionDto
    {
        [Required(ErrorMessage = "Prompt is required")]
        [StringLength(500, ErrorMessage = "Prompt cannot exceed 500 characters")]
        public required string Prompt { get; set; }

        [Required(ErrorMessage = "Question type is required")]
        public QuestionType QuestionType { get; set; }

        //[EnsureMinimumElements(2, ErrorMessage = "Multiple choice questions require at least 2 options")]
        [Required(ErrorMessage = "At least one option is required")]
        public List<string> Options { get; set; } = new List<string>();

        [Required(ErrorMessage = "Correct answer is required")]
        public required string CorrectAnswer { get; set; }

        [StringLength(1000, ErrorMessage = "Explanation cannot exceed 1000 characters")]
        public string? Explanation { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "VocabularyId must be a positive integer")]
        public int VocabularyId { get; set; } 
    }
}
