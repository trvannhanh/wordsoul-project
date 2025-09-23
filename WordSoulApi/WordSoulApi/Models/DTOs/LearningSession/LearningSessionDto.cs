namespace WordSoulApi.Models.DTOs.LearningSession
{
    public class LearningSessionDto
    {
        public int Id { get; set; }
        public List<int> VocabularyIds { get; set; } = new List<int>();
        public bool IsCompleted { get; set; }
        public int? PetId { get; set; }
        public double? CatchRate { get; set; }
    }
}
