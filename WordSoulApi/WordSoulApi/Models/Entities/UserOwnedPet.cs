namespace WordSoulApi.Models.Entities
{
    public class UserOwnedPet
    {
        public int UserId { get; set; } // Foreign key to User
        public User User { get; set; } // Navigation property to User
        public int PetId { get; set; } // Foreign key to Pet
        public Pet Pet { get; set; } // Navigation property to Pet
        public DateTime AcquiredAt { get; set; } = DateTime.UtcNow; 
        public bool IsFavorite { get; set; } = false; // Indicates if the pet is marked as a favorite by the user
        public bool IsActive { get; set; } = true; // Indicates if the pet is currently active for the user
    }
}
