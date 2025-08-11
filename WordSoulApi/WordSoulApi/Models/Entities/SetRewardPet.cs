namespace WordSoulApi.Models.Entities
{
    public class SetRewardPet
    {
        public int VocabularySetId { get; set; } // Foreign key to VocabularySet
        public VocabularySet VocabularySet { get; set; } // Navigation property to LearningSession
        public int PetId { get; set; } // Foreign key to Pet
        public Pet Pet { get; set; } // Navigation property to Pet
        public double DropRate { get; set; } = 1; // The drop rate for this pet in the vocabulary set, e.g., 0.05 for 5%
    }
}
