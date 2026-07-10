using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WordSoul.Application.DTOs.Pronunciation;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

using WordSoul.Application.Interfaces.Repositories;

namespace WordSoul.Application.Services
{
    public class PronunciationPracticeService : IPronunciationPracticeService
    {
        private readonly IAzurePronunciationService _azurePronunciationService;
        private readonly ISRSService _srsService;
        private readonly IDailyQuestService _dailyQuestService;
        private readonly IUserAchievementService _userAchievementService;
        private readonly IPetBuffService _petBuffService;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<PronunciationPracticeService> _logger;

        public PronunciationPracticeService(
            IAzurePronunciationService azurePronunciationService,
            ISRSService srsService,
            IDailyQuestService dailyQuestService,
            IUserAchievementService userAchievementService,
            IPetBuffService petBuffService,
            IUnitOfWork uow,
            ILogger<PronunciationPracticeService> logger)
        {
            _azurePronunciationService = azurePronunciationService;
            _srsService = srsService;
            _dailyQuestService = dailyQuestService;
            _userAchievementService = userAchievementService;
            _petBuffService = petBuffService;
            _uow = uow;
            _logger = logger;
        }

        public async Task<List<PronunciationWordDto>> GetWordsForPracticeAsync(
            int userId,
            int limit = 20,
            CancellationToken ct = default)
        {
            var progresses = await _uow.UserVocabularyProgress
                .GetAllUserVocabularyProgressByUserAsync(userId, ct);

            return progresses
                .Where(p => p.MemoryState != "New" && p.Vocabulary != null)
                .OrderByDescending(p => p.PronunciationWrongCount)       // Từ hay sai trước
                .ThenBy(p => p.RetentionScore)                            // Rồi từ yếu nhất
                .ThenBy(p => p.LastPronunciationAt ?? DateTime.MinValue) // Rồi từ lâu không luyện
                .Take(limit)
                .Select(p => new PronunciationWordDto
                {
                    VocabularyId = p.VocabularyId,
                    Word = p.Vocabulary!.Word,
                    Meaning = p.Vocabulary.Meaning,
                    IpaTranscription = p.Vocabulary.Pronunciation,
                    PronunciationUrl = p.Vocabulary.PronunciationUrl,
                    ExampleSentence = p.Vocabulary.ExampleSentence,
                    MemoryState = p.MemoryState,
                    PronunciationWrongCount = p.PronunciationWrongCount,
                    LastPronunciationAt = p.LastPronunciationAt
                })
                .ToList();
        }

        public async Task<PronunciationAssessResponse> AssessPronunciationAsync(
            int userId,
            int vocabularyId,
            string referenceWord,
            Stream audioStream,
            string contentType,
            int? petId,
            CancellationToken ct = default)
        {
            if (audioStream == null || audioStream.Length == 0)
                throw new ArgumentException("Audio data is required.", nameof(audioStream));

            if (string.IsNullOrWhiteSpace(referenceWord))
                throw new ArgumentException("Reference word is required.", nameof(referenceWord));

            if (vocabularyId <= 0)
                throw new ArgumentException("VocabularyId is required.", nameof(vocabularyId));

            // 1. Đọc audio bytes, convert WebM → WAV nếu cần
            byte[] audioBytes;
            using (var ms = new MemoryStream())
            {
                await audioStream.CopyToAsync(ms, ct);
                audioBytes = ms.ToArray();
            }

            // Convert WebM/Opus → WAV PCM 16kHz mono nếu client gửi WebM hoặc Ogg
            byte[] wavBytes;
            if (contentType.Contains("webm") || contentType.Contains("ogg"))
            {
                wavBytes = _azurePronunciationService.ConvertWebmToWav(audioBytes);
            }
            else
            {
                wavBytes = audioBytes; // Giả định đã là WAV
            }

            // 2. Gọi Azure Pronunciation Assessment
            using (var wavStream = new MemoryStream(wavBytes))
            {
                var assessment = await _azurePronunciationService.AssessAsync(
                    wavStream, referenceWord.Trim(), ct);

                var result = assessment.ToResult();

                // 3. Tính PetXpMultiplier từ active pet buff
                double petXpMultiplier = 1.0;
                if (petId.HasValue && petId.Value > 0)
                {
                    var petBuff = await _petBuffService.GetActivePetBuffAsync(userId, ct);
                    if (petBuff != null)
                    {
                        petXpMultiplier = petBuff.XpMultiplier;
                    }
                }

                // 4. Tính XP
                int baseXp = result switch
                {
                    PronunciationResult.Perfect => 10,
                    PronunciationResult.NearMiss => 3,
                    _ => 0
                };
                int finalXp = (int)Math.Floor(baseXp * petXpMultiplier);

                // 5. Lưu PronunciationAttempt
                var attempt = new PronunciationAttempt
                {
                    UserId = userId,
                    VocabularyId = vocabularyId,
                    AttemptTime = DateTime.UtcNow,
                    AccuracyScore = assessment.AccuracyScore,
                    FluencyScore = assessment.FluencyScore,
                    CompletenessScore = assessment.CompletenessScore,
                    PronunciationScore = assessment.PronunciationScore,
                    Result = result,
                    AzureRawResponse = assessment.RawJson,
                    XpAwarded = finalXp
                };
                await _uow.PronunciationAttempt.AddAsync(attempt, ct);

                // 6. Áp dụng hiệu ứng lên SM-2 (nhẹ, không thay đổi Repetition, không tự gọi SaveChanges)
                await _srsService.ApplyPronunciationEffectAsync(userId, vocabularyId, result, ct);

                // 7. Thưởng XP nếu > 0
                if (finalXp > 0)
                {
                    await _uow.User.UpdateUserXPAndAPAsync(userId, finalXp, 0, ct);
                }

                // 8. Cập nhật Daily Quest + Achievement (chỉ khi Perfect)
                if (result == PronunciationResult.Perfect)
                {
                    await _dailyQuestService.UpdateQuestProgressAsync(
                        userId, QuestType.Pronunciation, 1, null, ct);

                    await _userAchievementService.UpdateAchievementProgressAsync(
                        userId, ConditionType.PronunciationMastered, 1, ct);
                }

                // 9. Save all changes atomically in a single transaction commit
                await _uow.SaveChangesAsync(ct);

                // 10. Trả về kết quả
                return new PronunciationAssessResponse
                {
                    AccuracyScore = assessment.AccuracyScore,
                    FluencyScore = assessment.FluencyScore,
                    CompletenessScore = assessment.CompletenessScore,
                    PronunciationScore = assessment.PronunciationScore,
                    Result = result,
                    ResultLabel = result.ToString(),
                    XpAwarded = finalXp,
                    PetXpMultiplier = petXpMultiplier,
                    Phonemes = assessment.Phonemes
                        .Select(ph => new PhonemeResultDto
                        {
                            Phoneme = ph.Phoneme,
                            AccuracyScore = ph.AccuracyScore,
                            ResultLabel = ph.Result.ToString()
                        })
                        .ToList()
                };
            }
        }

        public async Task<List<PronunciationHistoryDto>> GetHistoryAsync(
            int userId,
            int vocabularyId,
            CancellationToken ct = default)
        {
            var history = await _uow.PronunciationAttempt
                .GetByUserAndVocabAsync(userId, vocabularyId, 10, ct);

            return history.Select(a => new PronunciationHistoryDto
            {
                AttemptTime = a.AttemptTime,
                PronunciationScore = a.PronunciationScore,
                Result = a.Result.ToString(),
                XpAwarded = a.XpAwarded
            }).ToList();
        }

        public async Task<PronunciationStatsDto> GetStatsAsync(
            int userId,
            CancellationToken ct = default)
        {
            return await _uow.PronunciationAttempt.GetUserStatsAsync(userId, ct);
        }
    }
}
