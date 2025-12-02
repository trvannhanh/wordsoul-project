using Microsoft.EntityFrameworkCore;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Domain.Entities;
using WordSoul.Infrastructure.Persistence;

namespace WordSoul.Infrastructure.Persistence.Repositories
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
        public async Task<IEnumerable<SessionVocabulary>> GetSessionVocabulariesBySessionIdAsync(int sessionId, CancellationToken cancellationToken = default)
        {
            return await _context.SessionVocabularies
                .AsNoTracking()
                .Include(sv => sv.Vocabulary) // Eager load Vocabulary để tránh N+1 query
                .Where(sv => sv.LearningSessionId == sessionId)
                .OrderBy(sv => sv.Order) // Sort theo thứ tự trong session
                .ToListAsync(cancellationToken);
        }

        // Lấy một SessionVocabulary cụ thể theo sessionId và vocabularyId
        public async Task<SessionVocabulary?> GetSessionVocabularyAsync(int sessionId, int vocabularyId, CancellationToken cancellationToken = default)
        {
            return await _context.SessionVocabularies
                .Include(sv => sv.Vocabulary) // Eager load để service không cần query thêm
                .FirstOrDefaultAsync(sv => sv.LearningSessionId == sessionId && sv.VocabularyId == vocabularyId, cancellationToken);
        }

        //-------------------------------------UPDATE-----------------------------------------
        // Cập nhật SessionVocabulary
        public async Task<SessionVocabulary?> UpdateSessionVocabularyAsync(SessionVocabulary sessionVocabulary, CancellationToken cancellationToken = default)
        {
            if (sessionVocabulary == null)
                return null;

            // Attach và mark modified để EF Core track changes
            _context.SessionVocabularies.Attach(sessionVocabulary);
            _context.Entry(sessionVocabulary).State = EntityState.Modified;

            return await Task.FromResult(sessionVocabulary);
        }
    }
}