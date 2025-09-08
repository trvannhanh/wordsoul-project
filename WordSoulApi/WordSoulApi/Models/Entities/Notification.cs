namespace WordSoulApi.Models.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public User User { get; set; }
        public string Title { get; set; }
        public NotificationType Type { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; } = false;
    }

    public enum NotificationType { 
        Review,
        Reward,
        Event,
    }
}
