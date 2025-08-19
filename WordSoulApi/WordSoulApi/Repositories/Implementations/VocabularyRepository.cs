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
        public async Task<IEnumerable<Vocabulary>> GetAllVocabulariesAsync()
        {
            return await _context.Vocabularies.ToListAsync();
        }
        public async Task<Vocabulary?> GetVocabularyByIdAsync(int id)
        {
            return await _context.Vocabularies.FindAsync(id);
        }
        public async Task<Vocabulary> CreateVocabularyAsync(Vocabulary vocabulary)
        {
            _context.Vocabularies.Add(vocabulary);
            await _context.SaveChangesAsync();
            return vocabulary;
        }
        public async Task<Vocabulary> UpdateVocabularyAsync(Vocabulary vocabulary)
        {
            _context.Vocabularies.Update(vocabulary);
            await _context.SaveChangesAsync();
            return vocabulary;
        }

        public async Task<bool> DeleteVocabularyAsync(int id)
        {
            var vocabulary = await _context.Vocabularies.FindAsync(id);
            if (vocabulary == null) return false;

            _context.Vocabularies.Remove(vocabulary);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Vocabulary>> GetVocabulariesByWordsAsync(List<string> words)
        {
            if (words == null || !words.Any())
                return new List<Vocabulary>();

            var nomalizedWords = words.Select(w => w.ToLower()).Distinct().ToList();

            return await _context.Vocabularies
                .Where(v => nomalizedWords.Contains(v.Word.ToLower()))
                .ToListAsync();
        }
    }
}
