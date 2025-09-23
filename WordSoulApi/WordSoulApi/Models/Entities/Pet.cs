
using System.ComponentModel.DataAnnotations;

namespace WordSoulApi.Models.Entities
{
    public class Pet
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public required string Name { get; set; }
        [MaxLength(300)]
        public string? Description { get; set; }
        [MaxLength(200)]
        public string? ImageUrl { get; set; }
        public PetRarity Rarity { get; set; }
        public PetType Type { get; set; } 
        public int? Order { get; set; }
        public int? BaseFormId { get; set; }
        public int? NextEvolutionId { get; set; }
        public int? RequiredLevel { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public List<UserOwnedPet> UserOwnedPets { get; set; } = new List<UserOwnedPet>(); // Collection of user pets associated with the pet
        public List<SetRewardPet> SetRewardPets { get; set; } = new List<SetRewardPet>(); // Collection of pet sets associated with the pet
    }

    public enum PetRarity
    {
        Common, // e.g., "Common"
        Uncommon, // e.g., "Uncommon"
        Rare, // e.g., "Rare"
        Epic, // e.g., "Epic"
        Legendary // e.g., "Legendary"
    }



    public enum PetType
    {
        Nomadica,
        Dynamora,
        Adornica,
        Velocira,
        Substitua,
        Connectara,
        Preposita,
        Exclamora
    }
}
