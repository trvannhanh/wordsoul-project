using Microsoft.EntityFrameworkCore;
using WordSoulApi.Data;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;

namespace WordSoulApi.Repositories.Implementations
{
    public class UserOwnedPetRepository : IUserOwnedPetRepository
    {
        private readonly WordSoulDbContext _context;
        public UserOwnedPetRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        // kiểm tra người dùng có sở hữu pet này chưa
        public async Task<bool> CheckPetOwnedByUserAsync(int userId, int petId)
        {
            return await _context.UserOwnedPets.AnyAsync(up => up.UserId == userId && up.PetId == petId);
        }

        // Thêm pet vào danh sách sở hữu của người dùng
        public async Task AddPetToUserAsync(UserOwnedPet userOwnedPet)
        {
            _context.UserOwnedPets.Add(userOwnedPet);
            await _context.SaveChangesAsync();
        }
    }
}
