using Microsoft.EntityFrameworkCore;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Domain.Entities;

namespace WordSoul.Infrastructure.Persistence.Repositories
{
    public class SetRewardPetRepository : ISetRewardPetRepository
    {
        private readonly WordSoulDbContext _context;

        public SetRewardPetRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        // Lấy danh sách pet theo VocabularySetId
        public async Task<IEnumerable<SetRewardPet>> GetPetsByVocabularySetIdAsync(int vocabularySetId, CancellationToken cancellationToken = default)
        {
            return await _context.SetRewardPets
                .Where(sp => sp.VocabularySetId == vocabularySetId)
                .Include(sp => sp.Pet)
                .ToListAsync(cancellationToken);
        }

        public async Task<SetRewardPet?> GetAsync(int vocabularySetId, int petId, CancellationToken ct = default)
            => await _context.SetRewardPets
                .Include(sp => sp.Pet)
                .FirstOrDefaultAsync(sp => sp.VocabularySetId == vocabularySetId && sp.PetId == petId, ct);

        public async Task AddAsync(SetRewardPet entry, CancellationToken ct = default)
            => await _context.SetRewardPets.AddAsync(entry, ct);

        public void Update(SetRewardPet entry) => _context.SetRewardPets.Update(entry);

        public void Remove(SetRewardPet entry) => _context.SetRewardPets.Remove(entry);
    }
}