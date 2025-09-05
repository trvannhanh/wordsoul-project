using System.ComponentModel.DataAnnotations;
using WordSoulApi.Models.Entities;

namespace WordSoulApi.Models.DTOs.Pet
{
    public class CreatePetDto
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public IFormFile? ImageFile { get; set; }
        public PetRarity Rarity { get; set; }
        public PetType Type { get; set; }
    }
}
