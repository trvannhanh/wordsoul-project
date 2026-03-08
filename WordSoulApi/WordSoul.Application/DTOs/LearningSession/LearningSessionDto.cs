namespace WordSoul.Application.DTOs.LearningSession
{
    public class LearningSessionDto
    {
        public int Id { get; set; }
        public List<int> VocabularyIds { get; set; } = [];
        public bool IsCompleted { get; set; }
        public int? PetId { get; set; }
        public int? BuffPetId { get; set; }
        public double? CatchRate { get; set; }
        public int CurrentCorrectAnswered { get; set; }

        public string? BuffName { get; set; }
        public string? BuffDescription { get; set; }
        public string? BuffIcon { get; set; }

        public double PetXpMultiplier { get; set; } 
        public double PetCatchBonus { get; set; } 
        public bool PetHintShield { get; set; } 
        public bool PetReducePenalty { get; set; } 
    }
}
