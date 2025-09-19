namespace WordSoulApi.Models.DTOs.Pet
{
    public class UserOwnedPetDto
    {

        public int UserId { get; set; } // Foreign key to User
        public int PetId { get; set; } // Foreign key to Pet
        public int Level { get; set; } = 1;
        public int Experience { get; set; } = 0;
        public bool IsFavorite { get; set; } = false; // Indicates if the pet is marked as a favorite by the user
        public bool IsActive { get; set; } = true; // Indicates if the pet is currently active for the user
        public DateTime AcquiredAt { get; set; } = DateTime.UtcNow;
    }
}
