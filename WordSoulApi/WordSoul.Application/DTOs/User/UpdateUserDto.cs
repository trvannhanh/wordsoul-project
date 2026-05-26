using System.ComponentModel.DataAnnotations;

namespace WordSoul.Application.DTOs.User
{
    public class UpdateUserDto
    {
        public string? Username { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class AssignRoleDto
    {
        [Required]
        public string RoleName { get; set; } = string.Empty;  // "Admin", "User", v.v.
    }

    public class UpdateUserStatusDto
    {
        [Required]
        public bool IsActive { get; set; }
    }
}
