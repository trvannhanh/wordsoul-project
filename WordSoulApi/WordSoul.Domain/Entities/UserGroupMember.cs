namespace WordSoul.Domain.Entities
{
    public class UserGroupMember
    {
        public int UserGroupId { get; set; }
        public UserGroup UserGroup { get; set; } = null!;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
