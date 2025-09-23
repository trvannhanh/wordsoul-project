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

        // -------------------------------------CREATE-----------------------------------------

        // Tạo bộ từ vựng mới
        public async Task<VocabularySet> CreateVocabularySetAsync(VocabularySet vocabularySet)
        {
            _context.VocabularySets.Add(vocabularySet);
            await _context.SaveChangesAsync();
            return vocabularySet;
        }

        //-------------------------------------READ-------------------------------------------

        // Lấy bộ từ vựng theo ID, bao gồm các từ vựng liên quan
        public async Task<VocabularySet?> GetVocabularySetByIdAsync(int id)
        {
            return await _context.VocabularySets
                .AsNoTracking()
                .Include(vs => vs.SetVocabularies)
                .ThenInclude(sv => sv.Vocabulary)
                .FirstOrDefaultAsync(vs => vs.Id == id);
        }


        // Tìm kiếm bộ từ vựng với các tiêu chí và phân trang
        public async Task<List<VocabularySet>> GetAllVocabularySetsAsync(
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
                .AsNoTracking()
                .Include(vs => vs.SetVocabularies)
                    .ThenInclude(sv => sv.Vocabulary)
                .Include(vs => vs.CreatedBy)
                .Include(vs => vs.UserVocabularySets)
                .AsSplitQuery()
                .OrderBy(vs => vs.Id)
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



        // ------------------------------------UPDATE-----------------------------------------

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

        // -------------------------------------DELETE------------------------------------------

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




    }
}
