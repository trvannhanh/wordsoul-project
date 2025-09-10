using System.ComponentModel.DataAnnotations;

namespace WordSoulApi.Models.DTOs.Pet
{
    public class AssignPetDto
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public int PetId { get; set; }
        public int InitialLevel { get; set; } = 1;  // Level ban đầu khi gán
        public int InitialExperience { get; set; } = 0;  // Experience ban đầu
        public bool IsFavorite { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }
}
