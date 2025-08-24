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

        // Lấy tất cả pet
        public async Task<IEnumerable<Pet>> GetAllPetsAsync()
        {
            return await _context.Pets.ToListAsync();
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
