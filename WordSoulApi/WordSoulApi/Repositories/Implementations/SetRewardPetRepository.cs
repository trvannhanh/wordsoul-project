using Microsoft.EntityFrameworkCore;
using WordSoulApi.Data;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;

namespace WordSoulApi.Repositories.Implementations
{
    public class SetRewardPetRepository : ISetRewardPetRepository
    {
        private readonly WordSoulDbContext _context;
        public SetRewardPetRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        // Lấy danh sách pet theo VocabularySetId
        public async Task<IEnumerable<SetRewardPet>> GetPetsByVocabularySetIdAsync(int vocabularySetId)
        {
            // 3. Lấy danh sách pet thuộc set
            var setPets = await _context.SetRewardPets
                .Where(sp => sp.VocabularySetId == vocabularySetId)
                .Include(sp => sp.Pet)
                .ToListAsync();

            return setPets;
        }
    }
}
