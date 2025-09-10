using CloudinaryDotNet.Actions;
using Microsoft.EntityFrameworkCore;
using WordSoulApi.Data;
using WordSoulApi.Models.DTOs.Vocabulary;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
        public async Task<IEnumerable<Vocabulary>> GetAllVocabulariesAsync(string? word, string? meaning, PartOfSpeech? partOfSpeech, CEFRLevel? cEFRLevel, int pageNumber, int pageSize)
        {

            var query = _context.Vocabularies
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(word))
                query = query.Where(v => v.Word.Contains(word));

            if (!string.IsNullOrWhiteSpace(meaning))
                query = query.Where(v => v.Meaning.Contains(meaning));

            if (partOfSpeech.HasValue)
                query = query.Where(vs => vs.PartOfSpeech == partOfSpeech.Value);

            if (cEFRLevel.HasValue)
                query = query.Where(vs => vs.CEFRLevel == cEFRLevel.Value);

            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
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

        //Kiểm tra từ vựng đã tồn tại trong bộ chưa
        public async Task<bool> CheckVocabularyExistFromSessionAsync(string word, int sessionId)
        {
            return await _context.SetVocabularies
            .AnyAsync(sv => sv.VocabularySetId == sessionId && sv.Vocabulary.Word == word);
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



    }
}
