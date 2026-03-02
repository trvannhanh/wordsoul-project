using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordSoul.Application.DTOs.SRS;

namespace WordSoul.Application.Interfaces.Services
{
    public interface ISRSService
    {
        /// <summary>
        /// Update progress after review session
        /// </summary>
        Task<SRSUpdateResult> UpdateAfterReviewAsync(
            int userId,
            int vocabularyId,
            int grade,
            CancellationToken ct = default);

        /// <summary>
        /// Get vocabularies due for review
        /// </summary>
        Task<List<VocabularyDueDto>> GetDueVocabulariesAsync(
            int userId,
            int limit = 20,
            CancellationToken ct = default);

        /// <summary>
        /// Calculate overall retention score for user
        /// </summary>
        Task<decimal> GetOverallRetentionScoreAsync(
            int userId,
            CancellationToken ct = default);
    }
}
