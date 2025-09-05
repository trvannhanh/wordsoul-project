using Microsoft.EntityFrameworkCore;
using WordSoulApi.Data;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;

namespace WordSoulApi.Repositories.Implementations
{
    public class PetRepository : IPetRepository
    {
        private readonly WordSoulDbContext _context;
        public PetRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<(Pet Pet, bool IsOwned)>> GetAllPetsAsync(
            int userId,
            string? name,
            PetRarity? rarity,
            PetType? type,
            bool? isOwned,
            int pageNumber,
            int pageSize)
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
                        Pet = x.Pet,
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

            // Pagination
            query = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            var result = await query.ToListAsync();

            return result.Select(x => (x.Pet, x.IsOwned));
        }

        // Lấy pet theo ID
        public async Task<Pet?> GetPetByIdAsync(int id)
        {
            return await _context.Pets.FindAsync(id);
        }

        // Tạo pet mới
        public async Task<Pet> CreatePetAsync(Pet pet)
        {
            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();
            return pet;
        }

        // Cập nhật pet
        public async Task<Pet> UpdatePetAsync(Pet pet)
        {
            _context.Pets.Update(pet);
            await _context.SaveChangesAsync();
            return pet;
        }

        // Xóa pet theo ID
        public async Task<bool> DeletePetAsync(int id)
        {
            var pet = await _context.Pets.FindAsync(id);
            if (pet == null) return false;
            _context.Pets.Remove(pet);
            return await _context.SaveChangesAsync() > 0;
        }

    }
}
