using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WordSoul.Application.DTOs.Pronunciation;
using WordSoul.Application.Interfaces.Repositories;

namespace WordSoul.Application.Interfaces.Services
{
    public interface IPronunciationPracticeService
    {
        Task<List<PronunciationWordDto>> GetWordsForPracticeAsync(
            int userId,
            int limit = 20,
            CancellationToken ct = default);

        Task<PronunciationAssessResponse> AssessPronunciationAsync(
            int userId,
            int vocabularyId,
            string referenceWord,
            Stream audioStream,
            string contentType,
            int? petId,
            CancellationToken ct = default);
        Task<List<PronunciationHistoryDto>> GetHistoryAsync(
            int userId,
            int vocabularyId,
            CancellationToken ct = default);

        Task<PronunciationStatsDto> GetStatsAsync(
            int userId,
            CancellationToken ct = default);
    }
}
