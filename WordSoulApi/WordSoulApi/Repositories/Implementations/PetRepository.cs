using Microsoft.EntityFrameworkCore;
using WordSoulApi.Data;
using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Implementations
{
    public class PetRepository : IPetRepository
    {
        private readonly WordSoulDbContext _context;
        public PetRepository(WordSoulDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Pet>> GetAllPetsAsync()
        {
            return await _context.Pets.ToListAsync();
        }
        public async Task<Pet?> GetPetByIdAsync(int id)
        {
            return await _context.Pets.FindAsync(id);
        }
        public async Task<Pet> CreatePetAsync(Pet pet)
        {
            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();
            return pet;
        }
        public async Task<Pet> UpdatePetAsync(Pet pet)
        {
            _context.Pets.Update(pet);
            await _context.SaveChangesAsync();
            return pet;
        }
        public async Task<bool> DeletePetAsync(int id)
        {
            var pet = await _context.Pets.FindAsync(id);
            if (pet == null) return false;
            _context.Pets.Remove(pet);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
