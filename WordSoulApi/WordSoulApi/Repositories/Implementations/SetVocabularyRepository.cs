using Microsoft.EntityFrameworkCore;
using WordSoulApi.Data;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;

namespace WordSoulApi.Repositories.Implementations
{
    public class SetVocabularyRepository : ISetVocabularyRepository
    {
        private readonly WordSoulDbContext _context;
        public SetVocabularyRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        //-------------------------------------CREATE-------------------------------------------

        // Tạo liên kết từ vựng vào bộ từ vựng
        public async Task<SetVocabulary> CreateSetVocabularyAsync(SetVocabulary setVocabulary)
        {
            _context.SetVocabularies.Add(setVocabulary);
            await _context.SaveChangesAsync();
            return setVocabulary;
        }

        //-------------------------------------READ-------------------------------------------

        // Lấy liên kết từ vựng và bộ từ vựng
        public async Task<SetVocabulary?> GetSetVocabularyAsync(int vocabId, int setId)
        {
            return await _context.SetVocabularies
                .FirstOrDefaultAsync(sv => sv.VocabularySetId == setId && sv.VocabularyId == vocabId);
        }

        //Lấy danh sách từ vựng của bộ từ vựng
        public async Task<(IEnumerable<Vocabulary> Vocabularies, int TotalCount)> GetVocabulariesFromSetAsync(int vocabularySetId, int pageNumber, int pageSize)
        {
            var query = _context.SetVocabularies
                .Include(sv => sv.Vocabulary)
                .Where(sv => sv.VocabularySetId == vocabularySetId)
                .OrderBy(sv => sv.Order);

            var totalCount = await query.CountAsync();

            var vocabularies = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(sv => sv.Vocabulary)
                .ToListAsync();

            return (vocabularies, totalCount);
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

        // Lấy các từ vựng chưa học từ một bộ từ cụ thể, ngẫu nhiên
        public async Task<IEnumerable<Vocabulary>> GetUnlearnedVocabulariesFromSetAsync(int userId, int setId, int take = 5)
        {
            // Lấy các từ vựng trong bộ mà người dùng chưa học
            // Sử dụng AsNoTracking để tối ưu hiệu suất khi chỉ đọc dữ liệu
            return await _context.SetVocabularies
                .AsNoTracking()
                .Where(sv => sv.VocabularySetId == setId)
                .Select(sv => sv.Vocabulary)
                .Where(v => !_context.UserVocabularyProgresses
                    .Any(uvp => uvp.UserId == userId && uvp.VocabularyId == v.Id))
                .Select(v => new Vocabulary { Id = v.Id }) // Chỉ lấy Id
                .OrderBy(v => Guid.NewGuid())
                .Take(take)
                .ToListAsync();
        }


        // Lấy các từ vựng cần ôn tập của user
        public async Task<IEnumerable<Vocabulary>> GetUnreviewdVocabulariesFromSetAsync(int userId, int take = 5)
        {
            return await _context.UserVocabularyProgresses
                .AsNoTracking()
                .Where(uvp => uvp.UserId == userId && uvp.NextReviewTime <= DateTime.UtcNow)
                .Select(uvp => uvp.Vocabulary)
                .Select(v => new Vocabulary { Id = v.Id })
                .Take(take)
                .ToListAsync();

        }



        //Lấy giá trị Order lớn nhất trong bảng SetVocabularies 
        public async Task<int> GetVocabularyOrderMaxAsync(int setId)
        {
            var max = await _context.SetVocabularies
                .Where(sv => sv.VocabularySetId == setId)
                .Select(sv => sv.Order)
                .DefaultIfEmpty(0)  // Tránh lỗi nếu không có record
                .MaxAsync();
            return max;
        }

        //-------------------------------------DELETE-------------------------------------------

        //Xóa liên kết từ vựng và bộ từ vựng
        public async Task<bool> DeleteSetVocabularyAsync(SetVocabulary setVocabulary)
        {
            var existingSetVocabulary = await _context.SetVocabularies.FindAsync(setVocabulary.VocabularySetId, setVocabulary.VocabularyId);
            if (existingSetVocabulary == null) return false;
            _context.SetVocabularies.Remove(existingSetVocabulary);
            return await _context.SaveChangesAsync() > 0;
        }

        //-------------------------------------OTHER------------------------------------------
        //Kiểm tra từ vựng đã tồn tại trong bộ chưa
        public async Task<bool> CheckVocabularyExistFromSetAsync(string word, int setId)
        {
            return await _context.SetVocabularies
                .AnyAsync(sv => sv.VocabularySetId == setId && sv.Vocabulary.Word == word);
        }

        // Đếm tổng từ vựng trong bộ
        public async Task<int> CountVocabulariesInSetAsync(int vocabularySetId)
        {
            return await _context.SetVocabularies
                .AsNoTracking()
                .CountAsync(sv => sv.VocabularySetId == vocabularySetId);
        }

    }
}
