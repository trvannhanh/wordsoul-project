using System.ComponentModel.DataAnnotations;

namespace WordSoul.Domain.Entities
{
    public class UserGroup
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public required string Name { get; set; }
        [MaxLength(500)]
        public string? Description { get; set; }
        public int CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<UserGroupMember> Members { get; set; } = [];
    }
}
