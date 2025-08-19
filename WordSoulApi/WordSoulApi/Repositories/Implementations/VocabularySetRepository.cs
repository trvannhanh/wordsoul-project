using Microsoft.EntityFrameworkCore;
using WordSoulApi.Data;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;

namespace WordSoulApi.Repositories.Implementations
{
    public class VocabularySetRepository : IVocabularySetRepository
    {
        private readonly WordSoulDbContext _context;

        public VocabularySetRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VocabularySet>> GetAllVocabularySetsAsync(int pageNumber = 1, int pageSize = 10)
        {
            return await _context.VocabularySets
                .AsNoTracking()
                .Include(vs => vs.SetVocabularies)
                .ThenInclude(sv => sv.Vocabulary)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<VocabularySet?> GetVocabularySetByIdAsync(int id)
        {
            return await _context.VocabularySets
                .AsNoTracking()
                .Include(vs => vs.SetVocabularies)
                .ThenInclude(sv => sv.Vocabulary)
                .FirstOrDefaultAsync(vs => vs.Id == id);
        }

        public async Task<VocabularySet> CreateVocabularySetAsync(VocabularySet vocabularySet)
        {
            _context.VocabularySets.Add(vocabularySet);
            await _context.SaveChangesAsync();
            return vocabularySet;
        }

        public async Task<VocabularySet?> UpdateVocabularySetAsync(VocabularySet vocabularySet)
        {
            var existingVocabularySet = await _context.VocabularySets
                .FirstOrDefaultAsync(vs => vs.Id == vocabularySet.Id);

            if (existingVocabularySet == null)
            {
                return null;
            }

            existingVocabularySet.Title = vocabularySet.Title;
            existingVocabularySet.Theme = vocabularySet.Theme;
            existingVocabularySet.Description = vocabularySet.Description;
            existingVocabularySet.DifficultyLevel = vocabularySet.DifficultyLevel;
            existingVocabularySet.IsActive = vocabularySet.IsActive;

            _context.VocabularySets.Update(existingVocabularySet);
            await _context.SaveChangesAsync();
            return existingVocabularySet;
        }

        public async Task<bool> DeleteVocabularySetAsync(int id)
        {
            var vocabularySet = await _context.VocabularySets
                .FirstOrDefaultAsync(vs => vs.Id == id);

            if (vocabularySet == null)
            {
                return false;
            }

            _context.VocabularySets.Remove(vocabularySet);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<VocabularySet>> SearchVocabularySetAsync(string? title, VocabularySetTheme? theme, VocabularyDifficultyLevel? difficulty,
                                                                    DateTime? createdAfter, int pageNumber, int pageSize)
        {
            var query = _context.VocabularySets
                .Include(vs => vs.SetVocabularies)
                    .ThenInclude(sv => sv.Vocabulary)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(vs => vs.Title.Contains(title));

            if (theme.HasValue)
                query = query.Where(vs => vs.Theme == theme.Value);

            if (difficulty.HasValue)
                query = query.Where(vs => vs.DifficultyLevel == difficulty.Value);

            if (createdAfter.HasValue)
                query = query.Where(vs => vs.CreatedAt >= createdAfter.Value);

            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
