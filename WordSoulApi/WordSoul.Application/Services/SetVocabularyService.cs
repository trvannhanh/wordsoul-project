using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Logging;
using WordSoul.Application.DTOs;
using WordSoul.Application.DTOs.Vocabulary;
using WordSoul.Application.DTOs.VocabularySet;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;

namespace WordSoul.Application.Services
{
    public class SetVocabularyService : ISetVocabularyService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<SetVocabularyService> _logger;

        public SetVocabularyService(
            IUnitOfWork uow,
            ILogger<SetVocabularyService> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        // ============================================================================
        // CREATE
        // ============================================================================

        /// <summary>
        /// Thêm một từ vựng mới vào bộ từ vựng. Nếu từ đã tồn tại trong bộ thì ném lỗi.
        /// </summary>
        /// <param name="setId">ID của bộ từ vựng.</param>
        /// <param name="vocabularyDto">Dữ liệu từ vựng cần thêm.</param>
        /// <param name="imageUrl">URL hình ảnh đã upload (nếu có).</param>
        /// <param name="cancellationToken">Token hủy thao tác.</param>
        /// <returns>AdminVocabularyDto của từ vừa được tạo.</returns>
        /// <exception cref="KeyNotFoundException">Khi bộ từ vựng không tồn tại.</exception>
        /// <exception cref="ArgumentException">Khi từ vựng đã tồn tại trong bộ.</exception>
        public async Task<AdminVocabularyDto?> AddVocabularyToSetAsync(
            int setId,
            CreateVocabularyInSetDto vocabularyDto,
            string? imageUrl,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Adding vocabulary '{Word}' to set ID {SetId}", vocabularyDto.Word, setId);

            // Kiểm tra bộ từ vựng có tồn tại không
            var vocabularySet = await _uow.VocabularySet.GetVocabularySetByIdAsync(setId, cancellationToken)
                ?? throw new KeyNotFoundException("Bộ từ vựng không tồn tại.");

            // Kiểm tra từ đã tồn tại trong bộ chưa
            var exists = await _uow.SetVocabulary.CheckVocabularyExistFromSetAsync(vocabularyDto.Word, setId, cancellationToken);
            if (exists)
                throw new ArgumentException("Từ vựng đã tồn tại trong bộ này.");

            // Tạo từ vựng mới
            var vocabulary = new Vocabulary
            {
                Word = vocabularyDto.Word,
                Meaning = vocabularyDto.Meaning,
                Pronunciation = vocabularyDto.Pronunciation,
                PartOfSpeech = vocabularyDto.PartOfSpeech,
                Description = vocabularyDto.Description,
                CEFRLevel = vocabularyDto.CEFRLevel,
                ExampleSentence = vocabularyDto.ExampleSentence,
                ImageUrl = imageUrl,
                PronunciationUrl = vocabularyDto.PronunciationUrl
            };

            await _uow.Vocabulary.CreateVocabularyAsync(vocabulary, cancellationToken);

            // Tạo liên kết SetVocabulary với thứ tự tăng dần
            var maxOrder = await _uow.SetVocabulary.GetVocabularyOrderMaxAsync(setId, cancellationToken);
            var setVocabulary = new SetVocabulary
            {
                VocabularySetId = setId,
                VocabularyId = vocabulary.Id,
                Order = maxOrder + 1
            };

            await _uow.SetVocabulary.CreateSetVocabularyAsync(setVocabulary, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);


            _logger.LogInformation("Successfully added vocabulary ID {VocabId} to set ID {SetId}", vocabulary.Id, setId);

            return new AdminVocabularyDto
            {
                Id = vocabulary.Id,
                Word = vocabulary.Word,
                Meaning = vocabulary.Meaning,
                Pronunciation = vocabulary.Pronunciation,
                PartOfSpeech = vocabulary.PartOfSpeech.ToString(),
                CEFRLevel = vocabulary.CEFRLevel.ToString(),
                Description = vocabulary.Description,
                ExampleSentence = vocabulary.ExampleSentence,
                ImageUrl = vocabulary.ImageUrl,
                PronunciationUrl = vocabulary.PronunciationUrl
            };
        }

        /// <summary>
        /// Thêm một từ vựng đã có sẵn (theo vocabId) vào bộ từ vựng. Chỉ owner được thực hiện.
        /// </summary>
        public async Task<bool> AddExistingVocabularyToSetAsync(
            int setId,
            int vocabId,
            int requestingUserId,
            CancellationToken cancellationToken = default)
        {
            var set = await _uow.VocabularySet.GetVocabularySetByIdAsync(setId, cancellationToken)
                ?? throw new KeyNotFoundException("Bộ từ vựng không tồn tại.");

            if (set.CreatedById != requestingUserId)
                throw new UnauthorizedAccessException("Bạn không có quyền chỉnh sửa bộ từ vựng này.");

            var vocab = await _uow.Vocabulary.GetVocabularyByIdAsync(vocabId, cancellationToken)
                ?? throw new KeyNotFoundException("Từ vựng không tồn tại.");

            // Kiểm tra đã có trong bộ chưa
            var existing = await _uow.SetVocabulary.GetSetVocabularyAsync(vocabId, setId, cancellationToken);
            if (existing != null)
                throw new ArgumentException("Từ vựng đã có trong bộ này.");

            var maxOrder = await _uow.SetVocabulary.GetVocabularyOrderMaxAsync(setId, cancellationToken);
            var link = new SetVocabulary
            {
                VocabularySetId = setId,
                VocabularyId = vocabId,
                Order = maxOrder + 1
            };
            await _uow.SetVocabulary.CreateSetVocabularyAsync(link, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Existing vocab {VocabId} added to set {SetId} by user {UserId}", vocabId, setId, requestingUserId);
            return true;
        }

        // ============================================================================
        // READ
        // ============================================================================

        /// <summary>
        /// Lấy danh sách từ vựng trong một bộ với phân trang.
        /// </summary>
        public async Task<PagedResult<VocabularyDto>> GetVocabulariesInSetAsync(
            int setId,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Get vocabularies in set {SetId} - Page {Page}/{PageSize}", setId, pageNumber, pageSize);

            // Kiểm tra bộ có tồn tại
            var setExists = await _uow.VocabularySet.GetVocabularySetByIdAsync(setId, cancellationToken)
                ?? throw new KeyNotFoundException("Bộ từ vựng không tồn tại.");

            var (vocabularies, totalCount) = await _uow.SetVocabulary.GetVocabulariesFromSetAsync(
                setId, pageNumber, pageSize, cancellationToken);

            var dtos = vocabularies.Select(v => new VocabularyDto
            {
                Id = v.Id,
                Word = v.Word,
                Meaning = v.Meaning,
                PartOfSpeech = v.PartOfSpeech.ToString(),
                CEFRLevel = v.CEFRLevel.ToString(),
                Description = v.Description,
                ExampleSentence = v.ExampleSentence,
                ImageUrl = v.ImageUrl,
                PronunciationUrl = v.PronunciationUrl
            }).ToList();

            var result = new PagedResult<VocabularyDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return result;
        }

        /// <summary>
        /// Lấy thông tin chi tiết đầy đủ của bộ từ vựng kèm danh sách từ (có phân trang).
        /// </summary>
        public async Task<VocabularySetFullDetailDto?> GetVocabularySetFullDetailsAsync(
            int id,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {

            var vocabularySet = await _uow.SetVocabulary.GetVocabularySetFullDetailsAsync(id, page, pageSize, cancellationToken);
            if (vocabularySet == null)
                return null;

            var totalVocabularies = await _uow.SetVocabulary.CountVocabulariesInSetAsync(id, cancellationToken);

            var result = new VocabularySetFullDetailDto
            {
                Id = vocabularySet.Id,
                Title = vocabularySet.Title,
                Theme = vocabularySet.Theme.ToString(),
                ImageUrl = vocabularySet.ImageUrl,
                Description = vocabularySet.Description,
                DifficultyLevel = vocabularySet.DifficultyLevel.ToString(),
                IsActive = vocabularySet.IsActive,
                IsPublic = vocabularySet.IsPublic,
                CreatedById = vocabularySet.CreatedById,
                CreatedByUsername = vocabularySet.CreatedBy?.Username,
                CreatedAt = vocabularySet.CreatedAt,
                TotalVocabularies = totalVocabularies,
                CurrentPage = page,
                PageSize = pageSize,
                // Override fields ưu tiên cao hơn giá trị gốc từ Vocabulary
                Vocabularies = vocabularySet.SetVocabularies.Select(sv => new VocabularyDetailDto
                {
                    Id = sv.VocabularyId,
                    Word = sv.Vocabulary.Word,
                    Meaning = sv.OverrideMeaning ?? sv.Vocabulary.Meaning,
                    ImageUrl = sv.Vocabulary.ImageUrl,
                    Pronunciation = sv.OverridePronunciation ?? sv.Vocabulary.Pronunciation,
                    PronunciationUrl = sv.Vocabulary.PronunciationUrl,
                    ExampleSentenceAudioUrl = sv.Vocabulary.ExampleSentenceAudioUrl,
                    PartOfSpeech = sv.Vocabulary.PartOfSpeech.ToString(),
                    ExampleSentence = sv.OverrideExampleSentence ?? sv.Vocabulary.ExampleSentence,
                    Description = sv.OverrideDescription ?? sv.Vocabulary.Description,
                    IsCustomEdited = sv.OverrideMeaning != null || sv.OverrideExampleSentence != null
                                  || sv.OverridePronunciation != null || sv.OverrideDescription != null,
                    OriginalMeaning = sv.OverrideMeaning != null ? sv.Vocabulary.Meaning : null
                }).ToList()
            };

            return result;
        }

        // ============================================================================
        // UPDATE
        // ============================================================================

        /// <summary>
        /// Cập nhật Override fields của một từ vựng trong bộ.
        /// Chỉ ghi vào SetVocabulary — không đụng bảng Vocabulary gốc.
        /// </summary>
        public async Task<VocabularyDetailDto?> UpdateVocabularyInSetAsync(
            int setId,
            int vocabId,
            UpdateVocabularyInSetDto dto,
            int requestingUserId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("UpdateVocabularyInSet: set={SetId}, vocab={VocabId}, user={UserId}", setId, vocabId, requestingUserId);

            // Lấy bộ từ vựng để validate owner
            var vocabularySet = await _uow.VocabularySet.GetVocabularySetByIdAsync(setId, cancellationToken)
                ?? throw new KeyNotFoundException($"Bộ từ vựng {setId} không tồn tại.");

            if (vocabularySet.CreatedById != requestingUserId)
                throw new UnauthorizedAccessException("Bạn không có quyền chỉnh sửa bộ từ vựng này.");

            var link = await _uow.SetVocabulary.GetSetVocabularyAsync(vocabId, setId, cancellationToken);
            if (link == null)
            {
                _logger.LogWarning("SetVocabulary link not found: vocab={VocabId} in set={SetId}", vocabId, setId);
                return null;
            }

            // Ghi Override — null nghĩa là xóa override (reset về giá trị gốc)
            link.OverrideMeaning = dto.OverrideMeaning;
            link.OverrideExampleSentence = dto.OverrideExampleSentence;
            link.OverridePronunciation = dto.OverridePronunciation;
            link.OverrideDescription = dto.OverrideDescription;

            await _uow.SetVocabulary.UpdateSetVocabularyAsync(link, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Override updated for vocab={VocabId} in set={SetId}", vocabId, setId);

            var vocab = link.Vocabulary!;
            return new VocabularyDetailDto
            {
                Id = link.VocabularyId,
                Word = vocab.Word,
                Meaning = link.OverrideMeaning ?? vocab.Meaning,
                ImageUrl = vocab.ImageUrl,
                Pronunciation = link.OverridePronunciation ?? vocab.Pronunciation,
                PronunciationUrl = vocab.PronunciationUrl,
                ExampleSentenceAudioUrl = vocab.ExampleSentenceAudioUrl,
                PartOfSpeech = vocab.PartOfSpeech.ToString(),
                ExampleSentence = link.OverrideExampleSentence ?? vocab.ExampleSentence,
                Description = link.OverrideDescription ?? vocab.Description,
                IsCustomEdited = link.OverrideMeaning != null || link.OverrideExampleSentence != null
                              || link.OverridePronunciation != null || link.OverrideDescription != null,
                OriginalMeaning = link.OverrideMeaning != null ? vocab.Meaning : null
            };
        }

        // ============================================================================
        // GET PROGRESS
        // ============================================================================

        /// <summary>
        /// Lấy thống kê tiến trình học của user với bộ từ vựng chỉ định.
        /// </summary>
        public async Task<VocabularySetProgressDto?> GetVocabularySetProgressAsync(
            int setId,
            int userId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("GetVocabularySetProgress: set={SetId}, user={UserId}", setId, userId);

            // Lấy UserVocabularySet
            var userSet = await _uow.UserVocabularySet.GetUserVocabularySetAsync(userId, setId, cancellationToken);

            // Lấy toàn bộ VocabularyId trong bộ
            var vocabIds = await _uow.SetVocabulary.GetVocabularyIdsInSetAsync(setId, cancellationToken);
            if (vocabIds.Count == 0) return null;

            // Lấy tất cả tiến trình của user với các từ trong bộ
            var allProgress = await _uow.UserVocabularyProgress.GetAllUserVocabularyProgressByUserAsync(userId, cancellationToken);
            var progressInSet = allProgress.Where(p => vocabIds.Contains(p.VocabularyId)).ToList();

            // Phân loại theo MemoryState
            var masteredCount   = progressInSet.Count(p => p.MemoryState == "Mastered");
            var reviewCount     = progressInSet.Count(p => p.MemoryState == "Review");
            var learningCount   = progressInSet.Count(p => p.MemoryState == "Learning");
            // New = từ trong bộ chưa có progress record
            var newCount = vocabIds.Count - progressInSet.Count;

            // Tính trung bình
            var avgRetention = progressInSet.Count > 0
                ? (double)progressInSet.Average(p => p.RetentionScore)
                : 0;
            var totalCorrect = progressInSet.Sum(
                p => p.InitialRecallCorrectCount);
            var totalWrong = progressInSet.Sum(
                p => p.InitialRecallCount - p.InitialRecallCorrectCount);
            var correctRate  = (totalCorrect + totalWrong) > 0
                ? (double)totalCorrect / (totalCorrect + totalWrong) * 100
                : 0;

            // Heatmap: nhóm VocabularyReviewHistory
            var heatmapDays = await GetConfigIntAsync("VocabSet.HeatmapDays", 30, cancellationToken);
            var since = DateTime.UtcNow.AddDays(-heatmapDays);
            var reviewHistories = await _uow.VocabularyReviewHistory.GetReviewHistoryForVocabsAsync(
                userId, vocabIds, since, cancellationToken);

            var heatmap = reviewHistories
                .GroupBy(r => DateOnly.FromDateTime(r.ReviewTime))
                .Select(g => new ActivityDay { Date = g.Key, ReviewCount = g.Count() })
                .OrderBy(a => a.Date)
                .ToList();

            // Top từ yếu nhất (RetentionScore thấp, đã học)
            var weakLimit = await GetConfigIntAsync("VocabSet.WeakWordsLimit", 5, cancellationToken);
            var weakVocabs = progressInSet
                .OrderBy(p => p.RetentionScore)
                .Take(weakLimit)
                .Select(p => new WeakVocabularyDto
                {
                    Id = p.VocabularyId,
                    Word = p.Vocabulary?.Word,
                    Meaning = p.Vocabulary?.Meaning,
                    RetentionScore = p.RetentionScore,
                    MemoryState = p.MemoryState
                })
                .ToList();

            return new VocabularySetProgressDto
            {
                TotalVocabularies = vocabIds.Count,
                MasteredCount = masteredCount,
                ReviewCount = reviewCount,
                LearningCount = learningCount,
                NewCount = newCount < 0 ? 0 : newCount,
                OverallRetentionScore = Math.Round(avgRetention, 1),
                CorrectRate = Math.Round(correctRate, 1),
                TotalCompletedSession = userSet?.TotalCompletedSession ?? 0,
                StartedAt = userSet?.CreatedAt,
                IsCompleted = userSet?.IsCompleted ?? false,
                ActivityHeatmap = heatmap,
                WeakVocabularies = weakVocabs,
                VocabMemoryStates = progressInSet.ToDictionary(p => p.VocabularyId, p => p.MemoryState ?? "Learning")
            };
        }

        // ============================================================================
        // DELETE
        // ============================================================================

        /// <summary>
        /// Xóa liên kết từ vựng khỏi bộ từ vựng (không xóa từ vựng thật).
        /// </summary>
        /// <returns>true nếu xóa thành công, false nếu không tìm thấy liên kết.</returns>
        public async Task<bool> RemoveVocabularyFromSetAsync(
            int setId,
            int vocabId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Removing vocabulary {VocabId} from set {SetId}", vocabId, setId);

            var link = await _uow.SetVocabulary.GetSetVocabularyAsync(vocabId, setId, cancellationToken);
            if (link == null)
            {
                _logger.LogWarning("Link not found: Vocab {VocabId} in Set {SetId}", vocabId, setId);
                return false;
            }

            var success = await _uow.SetVocabulary.DeleteSetVocabularyAsync(link, cancellationToken);
            if (success)
            {
                await _uow.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Successfully removed vocabulary {VocabId} from set {SetId}", vocabId, setId);
            }

            return success;
        }

        // ============================================================================
        // PRIVATE HELPERS
        // HELPERS
        // ============================================================================

        private async Task<int> GetConfigIntAsync(string key, int defaultValue, CancellationToken ct)
        {
            try
            {
                var config = await _uow.SystemConfiguration.GetByKeyAsync(key, ct);
                if (config != null && int.TryParse(config.Value, out var val))
                {
                    return val;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Lỗi khi lấy SystemConfiguration key={Key}", key);
            }
            return defaultValue;
        }
    }
}
