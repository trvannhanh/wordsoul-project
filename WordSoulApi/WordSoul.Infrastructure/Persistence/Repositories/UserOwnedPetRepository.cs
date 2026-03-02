using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Domain.Entities;

namespace WordSoul.Infrastructure.Persistence.Repositories
{
    public class UserOwnedPetRepository : IUserOwnedPetRepository
    {
        private readonly WordSoulDbContext _context;

        public UserOwnedPetRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        //-------------------------------------CREATE-----------------------------------------

        // Thêm pet vào danh sách sở hữu của người dùng
        public async Task CreateUserOwnedPetAsync(UserOwnedPet userOwnedPet, CancellationToken cancellationToken = default)
        {
            await _context.UserOwnedPets.AddAsync(userOwnedPet, cancellationToken);
        }

        //-------------------------------------READ-------------------------------------------

        // Lấy sự sở hữu pet của người dùng
        public async Task<UserOwnedPet?> GetUserOwnedPetByUserAndPetIdAsync(int userId, int petId, CancellationToken cancellationToken = default)
        {
            return await _context.UserOwnedPets
                .Include(uop => uop.Pet)
                .FirstOrDefaultAsync(uop => uop.UserId == userId && uop.PetId == petId, cancellationToken);
        }

        // Lấy tất cả pet sở hữu của người dùng
        public async Task<IEnumerable<UserOwnedPet>> GetAllUserOwnedPetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _context.UserOwnedPets
                .Where(uop => uop.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        // Lấy pet đang active của người dùng
        public async Task<Pet?> GetActivePetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            var userOwnedPets = await _context.UserOwnedPets
                .Include(p => p.Pet)
                .Where(p => p.UserId == userId && p.IsActive)
                .FirstOrDefaultAsync(cancellationToken);

            var activePet = userOwnedPets?.Pet;
            return activePet;
        }

        // Lấy pet ngẫu nhiên theo SetId dựa trên DropRate
        public async Task<Pet?> GetRandomPetBySetIdAsync(int vocabularySetId, CancellationToken cancellationToken = default)
        {
            var setPets = await _context.SetRewardPets
                .Include(sp => sp.Pet)
                .Where(sp => sp.VocabularySetId == vocabularySetId)
                .ToListAsync(cancellationToken);

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

            if (!eligiblePets.Any()) return null;

            var chosenPet = eligiblePets[random.Next(eligiblePets.Count)];

            return chosenPet;
        }

        //-------------------------------------UPDATE-----------------------------------------

        // Cập nhật sự sở hữu pet của người dùng
        public async Task UpdateUserOwnedPetAsync(UserOwnedPet userOwnedPet, CancellationToken cancellationToken = default)
        {
            _context.UserOwnedPets.Update(userOwnedPet);
            await Task.CompletedTask;
        }

        //-------------------------------------DELETE-----------------------------------------

        // Xóa sự sở hữu pet
        public async Task DeleteUserOwnedPetAsync(UserOwnedPet userOwnedPet, CancellationToken cancellationToken = default)
        {
            _context.UserOwnedPets.Remove(userOwnedPet);
            await Task.CompletedTask;
        }

        //-------------------------------------OTHER------------------------------------------

        // Kiểm tra người dùng có sở hữu pet này chưa
        public async Task<bool> CheckPetOwnedByUserAsync(int userId, int petId, CancellationToken cancellationToken = default)
        {
            return await _context.UserOwnedPets.AnyAsync(up => up.UserId == userId && up.PetId == petId, cancellationToken);
        }
    }
}