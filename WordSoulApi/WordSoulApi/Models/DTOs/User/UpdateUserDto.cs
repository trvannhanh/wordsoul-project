using System.ComponentModel.DataAnnotations;

namespace WordSoulApi.Models.DTOs.User
{
    public class UpdateUserDto
    {
        public string Username { get; set; } = string.Empty;
    }

    public class AssignRoleDto
    {
        [Required]
        public string RoleName { get; set; } = string.Empty;  // "Admin", "User", v.v.
    }
}
