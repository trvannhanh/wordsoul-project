using Microsoft.EntityFrameworkCore;
using WordSoulApi.Data;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;


namespace WordSoulApi.Repositories.Implementations
{
    public class UserOwnedPetRepository : IUserOwnedPetRepository
    {
        private readonly WordSoulDbContext _context;
        private readonly ILogger<UserOwnedPetRepository> _logger;
        public UserOwnedPetRepository(WordSoulDbContext context, ILogger<UserOwnedPetRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        //-------------------------------------CREATE-----------------------------------------

        // Thêm pet vào danh sách sở hữu của người dùng
        public async Task CreateUserOwnedPetAsync(UserOwnedPet userOwnedPet)
        {
            _context.UserOwnedPets.Add(userOwnedPet);
            await _context.SaveChangesAsync();
        }

        //-------------------------------------READ-------------------------------------------
        //Lấy sự sở hữu pet của người dùng
        public async Task<UserOwnedPet?> GetUserOwnedPetByUserAndPetIdAsync(int userId, int petId)
        {
            return await _context.UserOwnedPets
                .Include(uop => uop.Pet)
                .FirstOrDefaultAsync(uop => uop.UserId == userId && uop.PetId == petId);
        }

        // Lấy tất cả pet sở hữu của người dùng
        public async Task<IEnumerable<UserOwnedPet>> GetAllUserOwnedPetByUserIdAsync(int userId)
        {
            return await _context.UserOwnedPets
                .Where(uop => uop.UserId == userId)
                .ToListAsync();
                
        }

        public async Task<Pet?> GetRandomPetBySetIdAsync(int vocabularySetId)
        {
            var setPets = await _context.SetRewardPets
                .Include(sp => sp.Pet)
                .Where(sp => sp.VocabularySetId == vocabularySetId)
                .ToListAsync();

            if (!setPets.Any()) return null;

            var random = new Random(Guid.NewGuid().GetHashCode());
            var eligiblePets = new List<Pet>();
            foreach (var sp in setPets)
            {
                double roll = random.NextDouble(); // 0.0 -> 1.0
                if (roll <= sp.DropRate) // Kiểm tra nếu pet được chọn dựa trên DropRate
                {
                    eligiblePets.Add(sp.Pet);
                }
            }

            var chosenPet = eligiblePets[random.Next(eligiblePets.Count)];

            return chosenPet;
        }


        //-------------------------------------UPDATE-----------------------------------------

        //Cập nhật sự sở hữu pet của người dùng
        public async Task UpdateUserOwnedPetAsync(UserOwnedPet userOwnedPet)
        {
            _context.UserOwnedPets.Update(userOwnedPet);
            await _context.SaveChangesAsync();
        }


        //-------------------------------------DELETE-----------------------------------------

        //Xóa sự sở hữu pet 
        public async Task DeleteUserOwnedPetAsync(UserOwnedPet userOwnedPet)
        {
            _context.UserOwnedPets.Remove(userOwnedPet);
            await _context.SaveChangesAsync();
        }

        //-------------------------------------OTHER------------------------------------------
        // kiểm tra người dùng có sở hữu pet này chưa
        public async Task<bool> CheckPetOwnedByUserAsync(int userId, int petId)
        {
            return await _context.UserOwnedPets.AnyAsync(up => up.UserId == userId && up.PetId == petId);
        }


    }
}
