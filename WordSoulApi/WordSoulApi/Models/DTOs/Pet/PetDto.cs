using WordSoulApi.Models.Entities;

namespace WordSoulApi.Models.DTOs.Pet
{
    public class PetDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string ImageUrl { get; set; }
        public string Rarity { get; set; }
        public string Type { get; set; }
        
    }

    public class UserPetDto : PetDto
    {
        public bool? isOwned { get; set; }
    }

    public class AdminPetDto : PetDto
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}
