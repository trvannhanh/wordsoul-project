using System.ComponentModel.DataAnnotations;
using WordSoulApi.Models.Entities;

namespace WordSoulApi.Models.DTOs.User
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
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

   

    public class LeaderBoardDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int totalXP { get; set; }
        public int totalAP { get; set; }
    }


}
