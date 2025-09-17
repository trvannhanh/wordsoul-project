using System.ComponentModel.DataAnnotations;
using WordSoulApi.Models.Entities;

namespace WordSoulApi.Models.DTOs.User
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class AssignRoleDto
    {
        [Required]
        public string RoleName { get; set; } = string.Empty;  // "Admin", "User", v.v.
    }

    public class UserDetailDto : UserDto
    {
        public int Level { get; set; }
        public int TotalXP { get; set; }
        public int TotalAP { get; set; }
        public int StreakDays { get; set; }
        public int PetCount { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class UserProgressDto
    {
        // Section Welcome / Thông báo ôn tập
        public int ReviewWordCount { get; set; }
        public DateTime? NextReviewTime { get; set; }

        // Biểu đồ số lượng từ theo cấp độ thành thạo
        public List<LevelStatDto> VocabularyStats { get; set; } = new();
    }

    public class LevelStatDto
    {
        public int Level { get; set; }
        public int Count { get; set; }
    }


}
