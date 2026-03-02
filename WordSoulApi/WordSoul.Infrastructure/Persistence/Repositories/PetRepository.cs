using Microsoft.EntityFrameworkCore;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Infrastructure.Persistence.Repositories
{
    public class PetRepository : IPetRepository
    {
        private readonly WordSoulDbContext _context;

        public PetRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        //------------------------------- CREATE -----------------------------------
        // Tạo pet mới
        public async Task<Pet> CreatePetAsync(Pet pet, CancellationToken cancellationToken = default)
        {
            await _context.Pets.AddAsync(pet, cancellationToken);
            return pet;
        }

        //------------------------------- READ -----------------------------------
        // Lấy danh sách Pet với thông tin sở hữu của người dùng
        public async Task<IEnumerable<(Pet Pet, bool IsOwned)>> GetAllPetsAsync(
            int userId,
            string? name = null,
            PetRarity? rarity = null,
            PetType? type = null,
            bool? isOwned = null,
            int? vocabularySetId = null,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            // Base query: left join Pets với UserOwnedPets
            var query = _context.Pets
                .GroupJoin(
                    _context.UserOwnedPets.Where(up => up.UserId == userId),
                    pet => pet.Id,
                    userPet => userPet.PetId,
                    (pet, userPetGroup) => new { Pet = pet, UserPetGroup = userPetGroup }
                )
                .SelectMany(
                    x => x.UserPetGroup.DefaultIfEmpty(),
                    (x, userPet) => new
                    {
                        x.Pet,
                        IsOwned = userPet != null
                    }
                );

            // Filter theo name
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(x => x.Pet.Name.Contains(name));
            }

            // Filter theo rarity
            if (rarity.HasValue)
            {
                query = query.Where(x => x.Pet.Rarity == rarity.Value);
            }

            // Filter theo type
            if (type.HasValue)
            {
                query = query.Where(x => x.Pet.Type == type.Value);
            }

            // Filter theo isOwned
            if (isOwned.HasValue)
            {
                query = query.Where(x => x.IsOwned == isOwned.Value);
            }

            // Filter theo vocabularySetId và sắp xếp
            if (vocabularySetId.HasValue)
            {
                query = query.Where(x => x.Pet.SetRewardPets.Any(sp => sp.VocabularySetId == vocabularySetId.Value));
                query = query.OrderBy(x => x.Pet.Rarity).ThenBy(x => x.Pet.Order);
            }
            else
            {
                query = query.OrderBy(x => x.Pet.Order);
            }

            // Pagination
            query = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            var result = await query.ToListAsync(cancellationToken);

            return result.Select(x => (x.Pet, x.IsOwned));
        }

        // Lấy pet theo ID
        public async Task<Pet?> GetPetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Pets.FindAsync([id], cancellationToken);
        }

        // Lấy ngẫu nhiên một số pet theo rarity
        public async Task<List<Pet>> GetRandomPetsByRarityAsync(PetRarity rarity, int count, CancellationToken cancellationToken = default)
        {
            var pets = await _context.Pets
                .Where(p => p.Rarity == rarity && p.IsActive)
                .OrderBy(p => Guid.NewGuid()) // Ngẫu nhiên hóa
                .Take(count)
                .ToListAsync(cancellationToken);

            return pets;
        }

        //------------------------------- UPDATE -----------------------------------
        // Cập nhật pet
        public async Task<Pet> UpdatePetAsync(Pet pet, CancellationToken cancellationToken = default)
        {
            _context.Pets.Update(pet);
            return await Task.FromResult(pet);
        }

        //------------------------------- DELETE -----------------------------------
        // Xóa pet theo ID
        public async Task<bool> DeletePetAsync(int id, CancellationToken cancellationToken = default)
        {
            var pet = await _context.Pets.FindAsync([id], cancellationToken);
            if (pet == null) return false;

            _context.Pets.Remove(pet);
            return true;
        }
    }
}