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

        // Lấy tất cả các bộ từ vựng với phân trang
        public async Task<IEnumerable<VocabularySet>> GetAllVocabularySetsAsync(int pageNumber = 1, int pageSize = 10)
        {
            return await _context.VocabularySets
                .AsNoTracking()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // Lấy bộ từ vựng theo ID, bao gồm các từ vựng liên quan
        public async Task<VocabularySet?> GetVocabularySetByIdAsync(int id)
        {
            return await _context.VocabularySets
                .AsNoTracking()
                .Include(vs => vs.SetVocabularies)
                .ThenInclude(sv => sv.Vocabulary)
                .FirstOrDefaultAsync(vs => vs.Id == id);
        }

        // Lấy bộ từ vựng theo ID kèm chi tiết các từ vựng bên trong với phân trang
        public async Task<VocabularySet?> GetVocabularySetFullDetailsAsync(int id, int page, int pageSize)
        {
            return await _context.VocabularySets
                .AsNoTracking()
                .Include(vs => vs.SetVocabularies) // Include SetVocabularies at the root level
                .ThenInclude(sv => sv.Vocabulary)  // Then include the related Vocabulary
                .Where(vs => vs.Id == id)
                .Select(vs => new VocabularySet
                {
                    Id = vs.Id,
                    Title = vs.Title,
                    Theme = vs.Theme,
                    ImageUrl = vs.ImageUrl,
                    Description = vs.Description,
                    DifficultyLevel = vs.DifficultyLevel,
                    IsActive = vs.IsActive,
                    CreatedAt = vs.CreatedAt,
                    SetVocabularies = vs.SetVocabularies
                        .OrderBy(sv => sv.VocabularyId) // Consistent ordering
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }

        // Đếm tổng từ vựng trong bộ
        public async Task<int> CountVocabulariesInSetAsync(int vocabularySetId)
        {
            return await _context.SetVocabularies
                .AsNoTracking()
                .CountAsync(sv => sv.VocabularySetId == vocabularySetId);
        }

        // Tạo bộ từ vựng mới
        public async Task<VocabularySet> CreateVocabularySetAsync(VocabularySet vocabularySet)
        {
            _context.VocabularySets.Add(vocabularySet);
            await _context.SaveChangesAsync();
            return vocabularySet;
        }


        // Cập nhật bộ từ vựng
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


        // Xóa bộ từ vựng theo ID
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


        // Tìm kiếm bộ từ vựng với các tiêu chí và phân trang
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
