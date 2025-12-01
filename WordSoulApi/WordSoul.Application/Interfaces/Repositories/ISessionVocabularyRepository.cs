

using WordSoul.Domain.Entities;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface ISessionVocabularyRepository
    {
        //-------------------------------- READ -----------------------------------
        // Lấy tất cả SessionVocabulary theo sessionId
        Task<IEnumerable<SessionVocabulary>> GetSessionVocabulariesBySessionIdAsync(int sessionId);
        // Lấy SessionVocabulary theo sessionId và vocabularyId
        Task<SessionVocabulary?> GetSessionVocabularyAsync(int sessionId, int vocabularyId);
        //-------------------------------- UPDATE -----------------------------------
        Task<SessionVocabulary?> UpdateSessionVocabularyAsync(SessionVocabulary sessionVocabulary);
    }
}