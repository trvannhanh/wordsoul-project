namespace WordSoulApi.Models.Entities
{
    public class AnswerRecord
    {
        public int Id { get; set; } // Unique identifier for the answer record
        public int UserId { get; set; } // Foreign key to User
        public User User { get; set; } 
        public string Answer { get; set; } // The user's answer
        public bool IsCorrect { get; set; } // Indicates if the answer was correct
        public int AttemptCount { get; set; } = 1; // Number of attempts made by the user
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int QuizQuestionId { get; set; } // Foreign key to QuizQuestion
        public QuizQuestion QuizQuestion { get; set; } // Navigation property to QuizQuestion
        public int LearningSessionId { get; set; } // Foreign key to LearningSession
        public LearningSession LearningSession { get; set; } // Navigation property to LearningSession
    } 

}
