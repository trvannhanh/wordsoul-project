using System.ComponentModel.DataAnnotations;
using WordSoulApi.Models.Entities;

namespace WordSoulApi.Models.DTOs.ItemDto
{
    public class ItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? Type { get; set; }
    }

    public class CreateItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IFormFile? ImageFile { get; set; }
        [Required(ErrorMessage = "Theme is required")]
        public ItemType Type { get; set; }
    }
}
