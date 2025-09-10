using System.ComponentModel.DataAnnotations;
using WordSoulApi.Models.Entities;

namespace WordSoulApi.Models.DTOs.User
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class AssignRoleDto
    {
        [Required]
        public string RoleName { get; set; } = string.Empty;  // "Admin", "User", v.v.
    }
}
