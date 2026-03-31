

using Microsoft.AspNetCore.Http;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.DTOs.Pet
{
    public class UpdatePetDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public IFormFile? ImageFile { get; set; }
        public PetRarity Rarity { get; set; }
        public PetType Type { get; set; }
        public PetType? SecondaryType { get; set; }
        public int? BaseFormId { get; set; }
        public int? NextEvolutionId { get; set; }
        public int? RequiredLevel { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}
