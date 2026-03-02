

using System.ComponentModel.DataAnnotations;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.DTOs.Achievement
{
    public class AchievementDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ConditionType { get; set; }
        public int ConditionValue { get; set; }
        public string? ItemName { get; set; }
        public string? ItemImageUrl { get; set; }
    }


    public class CreateAchievementDto
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public ConditionType ConditionType { get; set; }

        [Range(1, int.MaxValue)]
        public int ConditionValue { get; set; }

        [Range(1, int.MaxValue)]
        public int ItemId { get; set; }
    }
}
