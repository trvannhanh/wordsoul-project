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
        private readonly ILogger<VocabularyRepository> _logger;
        public VocabularyRepository(WordSoulDbContext context, ILogger<VocabularyRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // -------------------------------------CREATE-----------------------------------------

        // Tạo từ vựng mới
        public async Task<Vocabulary> CreateVocabularyAsync(Vocabulary vocabulary)
        {
            _context.Vocabularies.Add(vocabulary);
            await _context.SaveChangesAsync();
            return vocabulary;
        }

        //-------------------------------------READ-------------------------------------------
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



        // Lấy từ vựng theo ID
        public async Task<Vocabulary?> GetVocabularyByIdAsync(int id)
        {
            return await _context.Vocabularies.FindAsync(id);
        }


        // Lấy các từ vựng theo danh sách từ
        public async Task<IEnumerable<Vocabulary>> GetVocabulariesByWordsAsync(List<string> words)
        {
            if (words == null || !words.Any())
            {
                _logger.LogWarning("Empty or null word list provided for search.");
                return new List<Vocabulary>();
            }

            var nomalizedWords = words.Select(w => w.ToLower()).Distinct().ToList();

            return await _context.Vocabularies
                .Where(v => nomalizedWords.Contains(v.Word.ToLower()))
                .ToListAsync();
        }





        // -------------------------------------UPDATE-----------------------------------------
        // Cập nhật từ vựng
        public async Task<Vocabulary> UpdateVocabularyAsync(Vocabulary vocabulary)
        {
            _context.Vocabularies.Update(vocabulary);
            await _context.SaveChangesAsync();
            return vocabulary;
        }

        //-------------------------------------DELETE-----------------------------------------
        // Xóa từ vựng theo ID
        public async Task<bool> DeleteVocabularyAsync(int id)
        {
            var vocabulary = await _context.Vocabularies.FindAsync(id);
            if (vocabulary == null) return false;

            _context.Vocabularies.Remove(vocabulary);
            return await _context.SaveChangesAsync() > 0;
        }





    }
}
