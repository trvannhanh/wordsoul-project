using Microsoft.EntityFrameworkCore;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Domain.Entities;

namespace WordSoul.Infrastructure.Persistence.Repositories
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
        public async Task<SetVocabulary> CreateSetVocabularyAsync(SetVocabulary setVocabulary, CancellationToken cancellationToken = default)
        {
            await _context.SetVocabularies.AddAsync(setVocabulary, cancellationToken);
            return setVocabulary;
        }

        //-------------------------------------READ-------------------------------------------

        // Lấy liên kết từ vựng và bộ từ vựng
        public async Task<SetVocabulary?> GetSetVocabularyAsync(int vocabId, int setId, CancellationToken cancellationToken = default)
        {
            return await _context.SetVocabularies
                .FirstOrDefaultAsync(sv => sv.VocabularySetId == setId && sv.VocabularyId == vocabId, cancellationToken);
        }

        // Lấy danh sách từ vựng của bộ từ vựng
        public async Task<(IEnumerable<Vocabulary> Vocabularies, int TotalCount)> GetVocabulariesFromSetAsync(int vocabularySetId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _context.SetVocabularies
                .Include(sv => sv.Vocabulary)
                .Where(sv => sv.VocabularySetId == vocabularySetId)
                .OrderBy(sv => sv.Order);

            var totalCount = await query.CountAsync(cancellationToken);

            var vocabularies = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(sv => sv.Vocabulary)
                .ToListAsync(cancellationToken);

            return (vocabularies, totalCount);
        }

        // Lấy bộ từ vựng theo ID kèm chi tiết các từ vựng bên trong với phân trang
        public async Task<VocabularySet?> GetVocabularySetFullDetailsAsync(int id, int page, int pageSize, CancellationToken cancellationToken = default)
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
                .FirstOrDefaultAsync(cancellationToken);
        }

        // Lấy các từ vựng chưa học từ một bộ từ cụ thể, ngẫu nhiên
        public async Task<IEnumerable<Vocabulary>> GetUnlearnedVocabulariesFromSetAsync(int userId, int setId, int take = 5, CancellationToken cancellationToken = default)
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
                .ToListAsync(cancellationToken);
        }

        // Lấy các từ vựng cần ôn tập của user
        public async Task<IEnumerable<Vocabulary>> GetUnreviewdVocabulariesFromSetAsync(int userId, int take = 5, CancellationToken cancellationToken = default)
        {
            return await _context.UserVocabularyProgresses
                .AsNoTracking()
                .Where(uvp => uvp.UserId == userId && uvp.NextReviewTime <= DateTime.UtcNow)
                .Select(uvp => uvp.Vocabulary)
                .Select(v => new Vocabulary { Id = v.Id })
                .OrderBy(v => v.Id)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        // Lấy giá trị Order lớn nhất trong bảng SetVocabularies
        public async Task<int> GetVocabularyOrderMaxAsync(int setId, CancellationToken cancellationToken = default)
        {
            var max = await _context.SetVocabularies
                .Where(sv => sv.VocabularySetId == setId)
                .Select(sv => sv.Order)
                .DefaultIfEmpty(0)  // Tránh lỗi nếu không có record
                .MaxAsync(cancellationToken);
            return max;
        }

        //-------------------------------------DELETE-------------------------------------------

        // Xóa liên kết từ vựng và bộ từ vựng
        public async Task<bool> DeleteSetVocabularyAsync(SetVocabulary setVocabulary, CancellationToken cancellationToken = default)
        {
            var existingSetVocabulary = await _context.SetVocabularies.FindAsync(
                [setVocabulary.VocabularySetId, setVocabulary.VocabularyId],
                cancellationToken);

            if (existingSetVocabulary == null) return false;

            _context.SetVocabularies.Remove(existingSetVocabulary);
            return true;
        }

        //-------------------------------------OTHER------------------------------------------

        // Kiểm tra từ vựng đã tồn tại trong bộ chưa
        public async Task<bool> CheckVocabularyExistFromSetAsync(string word, int setId, CancellationToken cancellationToken = default)
        {
            return await _context.SetVocabularies
                .AnyAsync(sv => sv.VocabularySetId == setId && sv.Vocabulary.Word == word, cancellationToken);
        }

        // Đếm tổng từ vựng trong bộ
        public async Task<int> CountVocabulariesInSetAsync(int vocabularySetId, CancellationToken cancellationToken = default)
        {
            return await _context.SetVocabularies
                .AsNoTracking()
                .CountAsync(sv => sv.VocabularySetId == vocabularySetId, cancellationToken);
        }
    }
}