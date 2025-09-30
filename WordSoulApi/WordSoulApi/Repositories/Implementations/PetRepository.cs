using Microsoft.EntityFrameworkCore;
using WordSoulApi.Data;
using WordSoulApi.Filters;
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

        //------------------------------- CREATE -----------------------------------
        // Tạo pet mới
        public async Task<Pet> CreatePetAsync(Pet pet)
        {
            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();
            return pet;
        }



        //------------------------------- READ -----------------------------------
        // Lấy danh sách Pet cho người dùng
        public async Task<IEnumerable<(Pet Pet, bool IsOwned)>> GetAllPetsAsync(
            int userId,
            PetFilter filter)
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
            if (!string.IsNullOrEmpty(filter.Name))
            {
                query = query.Where(x => x.Pet.Name.Contains(filter.Name));
            }

            // Filter theo rarity
            if (filter.Rarity.HasValue)
            {
                query = query.Where(x => x.Pet.Rarity == filter.Rarity.Value);
            }

            // Filter theo type
            if (filter.Type.HasValue)
            {
                query = query.Where(x => x.Pet.Type == filter.Type.Value);
            }

            // Filter theo isOwned
            if (filter.IsOwned.HasValue)
            {
                query = query.Where(x => x.IsOwned == filter.IsOwned.Value);
            }

            if (filter.VocabularySetId.HasValue)
            {
                query = query.Where(x => x.Pet.SetRewardPets.Any(sp => sp.VocabularySetId == filter.VocabularySetId.Value));

                query = query.OrderBy(x => x.Pet.Rarity).ThenBy(x => x.Pet.Order);
            }
            else
            {
                query = query.OrderBy(x => x.Pet.Order);
            }

            
            // Pagination
            query = query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize);

            var result = await query.ToListAsync();

            return result.Select(x => (x.Pet, x.IsOwned));
        }




        // Lấy pet theo ID
        public async Task<Pet?> GetPetByIdAsync(int id)
        {
            return await _context.Pets.FindAsync(id);
        }

        // Lấy ngẫu nhiên một số pet theo rarity
        public async Task<List<Pet>> GetRandomPetsByRarityAsync(PetRarity rarity, int count)
        {
            var pets = await _context.Pets
                .Where(p => p.Rarity == rarity && p.IsActive)
                .OrderBy(p => Guid.NewGuid()) // Ngẫu nhiên hóa
                .Take(count)
                .ToListAsync();

            return pets;
        }

        //------------------------------- UPDATE -----------------------------------
        // Cập nhật pet
        public async Task<Pet> UpdatePetAsync(Pet pet)
        {
            _context.Pets.Update(pet);
            await _context.SaveChangesAsync();
            return pet;
        }

        //------------------------------- DELETE -----------------------------------

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
    