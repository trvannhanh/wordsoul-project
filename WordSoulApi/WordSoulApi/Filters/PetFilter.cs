using WordSoul.Domain.Enums;

namespace WordSoul.Api.Filters
{
    public class PetFilter
    {
        public string? Name { get; set; }
        public PetRarity? Rarity { get; set; }
        public PetType? Type { get; set; }
        public bool? IsOwned { get; set; }
        public int? VocabularySetId { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
