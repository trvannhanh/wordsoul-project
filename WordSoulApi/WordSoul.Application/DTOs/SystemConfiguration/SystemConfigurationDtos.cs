using System.ComponentModel.DataAnnotations;

namespace WordSoul.Application.DTOs.SystemConfiguration
{
    public class CreateSystemConfigurationDto
    {
        [Required(ErrorMessage = "Key is required")]
        [MaxLength(100, ErrorMessage = "Key cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Key only allows alphanumeric characters and underscores.")]
        public required string Key { get; set; }

        [Required(ErrorMessage = "Value is required")]
        [MaxLength(500, ErrorMessage = "Value cannot exceed 500 characters")]
        public required string Value { get; set; }

        [Required(ErrorMessage = "DataType is required")]
        [MaxLength(50, ErrorMessage = "DataType cannot exceed 50 characters")]
        public required string DataType { get; set; } // Boolean, Integer, Float, String

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [MaxLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        public string? Category { get; set; } // GENERAL, SRS, GAME_BALANCE, SYSTEM
    }

    public class UpdateSystemConfigurationDto
    {
        [Required(ErrorMessage = "Value is required")]
        [MaxLength(500, ErrorMessage = "Value cannot exceed 500 characters")]
        public required string Value { get; set; }

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [MaxLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        public string? Category { get; set; }
    }
}
