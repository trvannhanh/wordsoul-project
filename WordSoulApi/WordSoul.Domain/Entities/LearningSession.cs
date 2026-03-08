using WordSoul.Domain.Enums;

namespace WordSoul.Domain.Entities
{
    public class LearningSession
    {
        public int Id { get; set; }
        public SessionType Type { get; set; }
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime EndTime { get; set; } = DateTime.UtcNow.AddHours(1); 
        public bool IsCompleted { get; set; } = false;
        public int UserId { get; set; }
        public User? User { get; set; } // Navigation property to User
        public int? VocabularySetId { get; set; }
        public VocabularySet? VocabularySet { get; set; } // Navigation property to VocabularySet
        public int? PetId { get; set; } // Optional reference to a Pet
        public int? BuffPetId { get; set; }
        public Pet? Pet { get; set; } // Navigation property to Pet
        public double? CatchRate { get; set; }

        public string? BuffName { get; set; }
        public string? BuffDescription { get; set; }
        public string? BuffIcon { get; set; }

        public double PetXpMultiplier { get; set; } = 1.0;
        public double PetCatchBonus { get; set; } = 0;
        public bool PetHintShield { get; set; } = false;
        public bool PetReducePenalty { get; set; } = false;


        public List<SessionVocabulary> SessionVocabularies { get; set; } = [];
        public List<AnswerRecord> AnswerRecords { get; set; } = [];
    }
    
}
