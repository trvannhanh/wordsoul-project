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
        public async Task<IEnumerable<VocabularySet>> GetAllVocabularySetsAsync(
        string? title,
        VocabularySetTheme? theme,
        VocabularyDifficultyLevel? difficulty,
        DateTime? createdAfter,
        bool? isOwned,
        int? userId,
        int pageNumber,
        int pageSize)
        {
            var query = _context.VocabularySets
                .Include(vs => vs.SetVocabularies)
                    .ThenInclude(sv => sv.Vocabulary)
                .Include(vs => vs.CreatedBy)
                .Include(vs => vs.UserVocabularySets)
                .AsQueryable();

            // Chỉ lấy các set công khai hoặc thuộc sở hữu của user
            if (userId.HasValue)
            {
                query = query.Where(vs => vs.IsPublic || vs.UserVocabularySets.Any(uvs => uvs.UserId == userId.Value && uvs.IsActive));
            }
            else
            {
                query = query.Where(vs => vs.IsPublic); // Chưa đăng nhập: chỉ lấy IsPublic = true
            }

            // Các bộ lọc hiện có
            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(vs => vs.Title.Contains(title));

            if (theme.HasValue)
                query = query.Where(vs => vs.Theme == theme.Value);

            if (difficulty.HasValue)
                query = query.Where(vs => vs.DifficultyLevel == difficulty.Value);

            if (createdAfter.HasValue)
                query = query.Where(vs => vs.CreatedAt >= createdAfter.Value);

            // Bộ lọc isOwned (chỉ áp dụng khi đã đăng nhập)
            if (userId.HasValue && isOwned.HasValue)
            {
                if (isOwned.Value)
                {
                    query = query.Where(vs => vs.UserVocabularySets.Any(uvs => uvs.UserId == userId.Value && uvs.IsActive));
                }
                else
                {
                    query = query.Where(vs => !vs.UserVocabularySets.Any(uvs => uvs.UserId == userId.Value && uvs.IsActive));
                }
            }

            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // Tạo liên kết từ vựng vào bộ từ vựng
        public async Task<SetVocabulary> CreateSetVocabularyAsync(SetVocabulary setVocabulary)
        {
            _context.SetVocabularies.Add(setVocabulary);
            await _context.SaveChangesAsync();
            return setVocabulary;
        }

        // Lấy liên kết từ vựng và bộ từ vựng
        public async Task<SetVocabulary?> GetSetVocabularyAsync(int vocabId, int setId)
        {
            return await _context.SetVocabularies
                .FirstOrDefaultAsync(sv => sv.VocabularySetId == setId && sv.VocabularyId == vocabId);
        }

        //Xóa liên kết từ vựng và bộ từ vựng
        public async Task<bool> DeleteSetVocabularyAsync(SetVocabulary setVocabulary)
        {
            var existingSetVocabulary = await _context.SetVocabularies.FindAsync(setVocabulary.VocabularySetId, setVocabulary.VocabularyId);
            if (existingSetVocabulary == null) return false;
            _context.SetVocabularies.Remove(existingSetVocabulary);
            return await _context.SaveChangesAsync() > 0;
        }

    }
}
