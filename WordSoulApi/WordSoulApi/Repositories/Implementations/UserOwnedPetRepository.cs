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
        public async Task CreateUserOwnedPetAsync(UserOwnedPet userOwnedPet)
        {
            _context.UserOwnedPets.Add(userOwnedPet);
            await _context.SaveChangesAsync();
        }

        //Lấy sự sở hữu pet của người dùng
        public async Task<UserOwnedPet?> GetUserOwnedPetByUserAndPetIdAsync(int userId, int petId)
        {
            return await _context.UserOwnedPets
                .Include(uop => uop.Pet)
                .FirstOrDefaultAsync(uop => uop.UserId == userId && uop.PetId == petId);
        }

        //Cập nhật sự sở hữu pet của người dùng
        public async Task UpdateUserOwnedPetAsync(UserOwnedPet userOwnedPet)
        {
            _context.UserOwnedPets.Update(userOwnedPet);
            await _context.SaveChangesAsync();
        }

        //Xóa sự sở hữu pet 
        public async Task DeleteUserOwnedPetAsync(UserOwnedPet userOwnedPet)
        {
            _context.UserOwnedPets.Remove(userOwnedPet);
            await _context.SaveChangesAsync();
        }

        //Kiểm tra sự sở hữu pet của người dùng
        public async Task<bool> CheckExistsUserOwnedPetAsync(int userId, int petId)
        {
            return await _context.UserOwnedPets
                .AnyAsync(uop => uop.UserId == userId && uop.PetId == petId);
        }

        //Lấy tất cả sự sở hữu pet của người dùng
        public async Task<List<UserOwnedPet>> GetAllUserOwnedPetByUserIdAsync(int userId)
        {
            return await _context.UserOwnedPets
                .Where(uop => uop.UserId == userId)
                .Include(uop => uop.Pet)
                .ToListAsync();
        }
    }
}
