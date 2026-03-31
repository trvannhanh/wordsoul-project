using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.DTOs.Pet
{
    public class CreatePetDto
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public IFormFile? ImageFile { get; set; }
        public PetRarity Rarity { get; set; }
        public PetType Type { get; set; }
        public PetType? SecondaryType { get; set; }
        public int? BaseFormId { get; set; }
        public int? NextEvolutionId { get; set; }
        public int? RequiredLevel { get; set; }

    }

    public class BulkCreatePetDto
    {
        [Required]
        public List<CreatePetDto> Pets { get; set; } = [];  // Danh sách pet cần tạo
    }
}
