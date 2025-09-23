using WordSoulApi.Models.Entities;

namespace WordSoulApi.Models.DTOs.Achievement
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
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public ConditionType ConditionType { get; set; }
        public int ConditionValue { get; set; }
        public int ItemId { get; set; }
    }
}
