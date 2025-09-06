using Microsoft.EntityFrameworkCore;
using WordSoulApi.Data;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;

namespace WordSoulApi.Repositories.Implementations
{
    public class VocabularyRepository : IVocabularyRepository
    {
        private readonly WordSoulDbContext _context;
        public VocabularyRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        // Lấy tất cả các từ vựng
        public async Task<IEnumerable<Vocabulary>> GetAllVocabulariesAsync()
        {
            return await _context.Vocabularies.ToListAsync();
        }

        // Lấy từ vựng theo ID
        public async Task<Vocabulary?> GetVocabularyByIdAsync(int id)
        {
            return await _context.Vocabularies.FindAsync(id);
        }

        // Tạo từ vựng mới
        public async Task<Vocabulary> CreateVocabularyAsync(Vocabulary vocabulary)
        {
            _context.Vocabularies.Add(vocabulary);
            await _context.SaveChangesAsync();
            return vocabulary;
        }

        // Cập nhật từ vựng
        public async Task<Vocabulary> UpdateVocabularyAsync(Vocabulary vocabulary)
        {
            _context.Vocabularies.Update(vocabulary);
            await _context.SaveChangesAsync();
            return vocabulary;
        }

        // Xóa từ vựng theo ID
        public async Task<bool> DeleteVocabularyAsync(int id)
        {
            var vocabulary = await _context.Vocabularies.FindAsync(id);
            if (vocabulary == null) return false;

            _context.Vocabularies.Remove(vocabulary);
            return await _context.SaveChangesAsync() > 0;
        }

        // Lấy các từ vựng theo danh sách từ
        public async Task<IEnumerable<Vocabulary>> GetVocabulariesByWordsAsync(List<string> words)
        {
            if (words == null || !words.Any())
                return new List<Vocabulary>();

            var nomalizedWords = words.Select(w => w.ToLower()).Distinct().ToList();

            return await _context.Vocabularies
                .Where(v => nomalizedWords.Contains(v.Word.ToLower()))
                .ToListAsync();
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
                .Select (v => new Vocabulary { Id = v.Id})
                .Take(take)
                .ToListAsync();

        }

        // Lấy danh sách ID từ vựng theo ID phiên học
        public async Task<IEnumerable<int>> GetVocabularyIdsBySessionIdAsync(int sessionId)
        {
            return await _context.SessionVocabularies
                .Where(sv => sv.LearningSessionId == sessionId)
                .Select(sv => sv.VocabularyId)
                .ToListAsync();
        }

        //Lấy danh sách từ vựng của phiên học
        public async Task<IEnumerable<Vocabulary>> GetVocabulariesBySessionIdAsync (int sessionId)
        {
            return await _context.SessionVocabularies
                .AsNoTracking()
                .Where(sv => sv.LearningSessionId == sessionId)
                .Select(sv => sv.Vocabulary)
                .ToListAsync();
        }

    }
}
