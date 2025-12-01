namespace WordSoul.Application.DTOs.LearningSession
{
    public class LearningSessionDto
    {
        public int Id { get; set; }
        public List<int> VocabularyIds { get; set; } = [];
        public bool IsCompleted { get; set; }
        public int? PetId { get; set; }
        public double? CatchRate { get; set; }
        public int CurrentCorrectAnswered { get; set; }
    }
}
