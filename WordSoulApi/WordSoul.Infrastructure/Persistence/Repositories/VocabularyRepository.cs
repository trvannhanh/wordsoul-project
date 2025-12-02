using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;
using WordSoul.Infrastructure.Persistence;

namespace WordSoul.Infrastructure.Persistence.Repositories
{
    public class VocabularyRepository : IVocabularyRepository
    {
        private readonly WordSoulDbContext _context;

        public VocabularyRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        // -------------------------------------CREATE-----------------------------------------
        // Tạo từ vựng mới
        public Task<Vocabulary> CreateVocabularyAsync(Vocabulary vocabulary, CancellationToken cancellationToken = default)
        {
            _context.Vocabularies.Add(vocabulary);
            return Task.FromResult(vocabulary);
        }

        //-------------------------------------READ-------------------------------------------
        // Lấy tất cả các từ vựng
        public async Task<List<Vocabulary>> GetAllVocabulariesAsync(string? word, string? meaning, PartOfSpeech? partOfSpeech, CEFRLevel? cEFRLevel, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _context.Vocabularies
                .AsNoTracking()
                .OrderBy(v => v.Id)
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
                .ToListAsync(cancellationToken);
        }

        // Lấy từ vựng theo ID
        public async Task<Vocabulary?> GetVocabularyByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Vocabularies.FindAsync(new object[] { id }, cancellationToken);
        }

        // Lấy các từ vựng theo danh sách từ
        public async Task<List<Vocabulary>> GetVocabulariesByWordsAsync(List<string> words, CancellationToken cancellationToken = default)
        {
            if (words == null || !words.Any())
            {
                return new List<Vocabulary>();
            }

            var nomalizedWords = words.Select(w => w.ToLower()).Distinct().ToList();

            return await _context.Vocabularies
                .Where(v => nomalizedWords.Contains(v.Word.ToLower()))
                .ToListAsync(cancellationToken);
        }

        // -------------------------------------UPDATE-----------------------------------------
        // Cập nhật từ vựng
        public Task<Vocabulary> UpdateVocabularyAsync(Vocabulary vocabulary, CancellationToken cancellationToken = default)
        {
            _context.Vocabularies.Update(vocabulary);
            return Task.FromResult(vocabulary);
        }

        //-------------------------------------DELETE-----------------------------------------
        // Xóa từ vựng theo ID
        public async Task<bool> DeleteVocabularyAsync(int id, CancellationToken cancellationToken = default)
        {
            var vocabulary = await _context.Vocabularies.FindAsync(new object[] { id }, cancellationToken);
            if (vocabulary == null) return false;

            _context.Vocabularies.Remove(vocabulary);
            return true;
        }
    }
}