using System.ComponentModel.DataAnnotations;
using WordSoulApi.Models.Entities;

namespace WordSoulApi.Models.DTOs.Pet
{
    public class PetDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string ImageUrl { get; set; }
        public string? Rarity { get; set; }
        public string? Type { get; set; }
        public int? BaseFormId { get; set; }
        public int? NextEvolutionId { get; set; }
        public int? RequiredLevel { get; set; }



    }

    public class UserPetDto : PetDto
    {
        public bool? IsOwned { get; set; }
    }

    public class UserPetDetailDto : PetDto
    {
        public int? Level { get; set; }
        public int? Experience { get; set; }
        public bool? IsFavorite { get; set; } = false;
        public bool? IsActive { get; set; } = true;
        public DateTime? AcquiredAt { get; set; }
    }

    public class AdminPetDto : PetDto
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }

    public class UpgradePetDto
    {
        public int PetId { get; set; }
        public int Experience { get; set; }
        public int Level { get; set; }
        public bool IsLevelUp { get; set; } 
        public bool IsEvolved { get; set; }
        public int AP { get; set; }

    }
}
