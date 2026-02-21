

namespace WordSoul.Domain.Entities
{
    public class UserDailyQuest
    {
        public int Id { get; set; } // Primary key

        public int UserId { get; set; } // Foreign key to User
        public User? User { get; set; } // Navigation property to User

        public int DailyQuestId { get; set; } // Foreign key to DailyQuest
        public DailyQuest? DailyQuest { get; set; } // Navigation property to DailyQuest

        public int Progress { get; set; } = 0; // Current progress of the user for this quest

        public bool IsCompleted { get; set; } = false; // Whether the quest is completed

        public bool IsClaimed { get; set; } = false; // Whether the reward has been claimed

        public DateTime QuestDate { get; set; }
    }
}