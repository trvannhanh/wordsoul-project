using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordSoulApi.Data;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;

namespace WordSoulApi.Repositories.Implementations
{
    public class SessionVocabularyRepository : ISessionVocabularyRepository
    {
        private readonly WordSoulDbContext _context;

        public SessionVocabularyRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        //-------------------------------------READ-----------------------------------------

        // Lấy tất cả SessionVocabulary theo session ID (với Vocabulary eager loaded)
        public async Task<IEnumerable<SessionVocabulary>> GetSessionVocabulariesBySessionIdAsync(int sessionId)
        {
            return await _context.SessionVocabularies
                .AsNoTracking()
                .Include(sv => sv.Vocabulary) // Eager load Vocabulary để tránh N+1 query
                .Where(sv => sv.LearningSessionId == sessionId)
                .OrderBy(sv => sv.Order) // Sort theo thứ tự trong session
                .ToListAsync();
        }

        // : Lấy một SessionVocabulary cụ thể theo sessionId và vocabularyId
        public async Task<SessionVocabulary?> GetSessionVocabularyAsync(int sessionId, int vocabularyId)
        {
            return await _context.SessionVocabularies
                .Include(sv => sv.Vocabulary) // Eager load để service không cần query thêm
                .FirstOrDefaultAsync(sv => sv.LearningSessionId == sessionId && sv.VocabularyId == vocabularyId);
        }

        // ✅ MỚI: Cập nhật SessionVocabulary (trả về entity đã update)
        public async Task<SessionVocabulary?> UpdateSessionVocabularyAsync(SessionVocabulary sessionVocabulary)
        {
            if (sessionVocabulary == null)
                return null;

            // Attach và mark modified để EF Core track changes
            _context.SessionVocabularies.Attach(sessionVocabulary);
            _context.Entry(sessionVocabulary).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                // Reload để đảm bảo data consistency
                await _context.Entry(sessionVocabulary).ReloadAsync();
                return sessionVocabulary;
            }
            catch (DbUpdateConcurrencyException)
            {
                // Handle concurrency nếu cần (optimistic locking)
                await _context.Entry(sessionVocabulary).ReloadAsync();
                return null;
            }
        }

    }
}