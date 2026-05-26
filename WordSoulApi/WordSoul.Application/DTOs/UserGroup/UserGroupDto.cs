using System.ComponentModel.DataAnnotations;

namespace WordSoul.Application.DTOs.UserGroup
{
    public class UserGroupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MemberCount { get; set; }
        public string CreatedByUsername { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class UserGroupDetailDto : UserGroupDto
    {
        public List<GroupMemberDto> Members { get; set; } = [];
    }

    public class GroupMemberDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
    }

    public class CreateUserGroupDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateUserGroupDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class AddGroupMemberDto
    {
        [Required]
        public int UserId { get; set; }
    }
}
