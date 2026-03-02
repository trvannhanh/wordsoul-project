using WordSoul.Domain.Enums;
namespace WordSoul.Domain.Entities
{
    public class Achievement
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public ConditionType ConditionType { get; set; }
        public int ConditionValue { get; set; }
        public int RewardItemId { get; set; }
        public Item? Item { get; set; } = null;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<UserAchievement> UserAchievements { get; set; } = [];
    }

}
