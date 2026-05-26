using Microsoft.Extensions.Logging;
using WordSoul.Application.DTOs.Vocabulary;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Services
{
    public class VocabularyService : IVocabularyService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<VocabularyService> _logger;

        public VocabularyService(
            IUnitOfWork uow,
            ILogger<VocabularyService> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        // ============================================================================
        // CREATE
        // ============================================================================

        /// <summary>
        /// Tạo từ vựng mới (dùng cho admin).
        /// </summary>
        public async Task<AdminVocabularyDto> CreateVocabularyAsync(
            CreateVocabularyDto dto,
            string? imageUrl,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Word))
                throw new ArgumentException("Word is required.", nameof(dto.Word));

            _logger.LogInformation("Creating new vocabulary: {Word}", dto.Word);

            var vocabulary = new Vocabulary
            {
                Word = dto.Word.Trim(),
                Meaning = dto.Meaning?.Trim(),
                Pronunciation = dto.Pronunciation?.Trim(),
                PartOfSpeech = dto.PartOfSpeech,
                CEFRLevel = dto.CEFRLevel,
                Description = dto.Description?.Trim(),
                ExampleSentence = dto.ExampleSentence?.Trim(),
                ImageUrl = imageUrl,
                PronunciationUrl = dto.PronunciationUrl?.Trim()
            };

            await _uow.Vocabulary.CreateVocabularyAsync(vocabulary, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Vocabulary created with ID {Id}", vocabulary.Id);

            return MapToAdminDto(vocabulary);
        }

        // ============================================================================
        // READ
        // ============================================================================

        /// <summary>
        /// Lấy danh sách từ vựng với bộ lọc và phân trang.
        /// </summary>
        public async Task<IEnumerable<VocabularyDto>> GetAllVocabulariesAsync(
            string? word = null,
            string? meaning = null,
            PartOfSpeech? partOfSpeech = null,
            CEFRLevel? cefrLevel = null,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default)
        {

            var entities = await _uow.Vocabulary.GetAllVocabulariesAsync(
                word, meaning, partOfSpeech, cefrLevel, pageNumber, pageSize, cancellationToken);

            var dtos = entities.Select(MapToDto).ToList();
            return dtos;
        }

        /// <summary>
        /// Lấy chi tiết một từ vựng theo ID.
        /// </summary>
        public async Task<VocabularyDto?> GetVocabularyByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _uow.Vocabulary.GetVocabularyByIdAsync(id, cancellationToken);
            if (entity == null)
                return null;

            return MapToDto(entity);
            }

        /// <summary>
        /// Lấy nhiều từ vựng theo danh sách từ (word list) – thường dùng khi tạo session học.
        /// </summary>
        public async Task<IEnumerable<VocabularyDto>> GetVocabulariesByWordsAsync(
            SearchVocabularyDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto?.Words == null || !dto.Words.Any())
                return Enumerable.Empty<VocabularyDto>();

            var orderedWords = dto.Words.OrderBy(w => w).ToList();
            var entities = await _uow.Vocabulary.GetVocabulariesByWordsAsync(orderedWords, null, cancellationToken);

            var dtos = entities.Select(MapToDto).ToList();
            return dtos;
        }

        // ============================================================================
        // UPDATE
        // ============================================================================

        /// <summary>
        /// Cập nhật từ vựng (admin only).
        /// </summary>
        public async Task<AdminVocabularyDto?> UpdateVocabularyAsync(
            int id,
            CreateVocabularyDto dto,
            string? imageUrl,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(dto.Word))
                throw new ArgumentException("Word is required.", nameof(dto.Word));

            _logger.LogInformation("Updating vocabulary ID {Id}", id);

            var entity = await _uow.Vocabulary.GetVocabularyByIdAsync(id, cancellationToken)
                ?? throw new KeyNotFoundException($"Vocabulary with ID {id} not found.");

            // Cập nhật các field
            entity.Word = dto.Word.Trim();
            entity.Meaning = dto.Meaning?.Trim();
            entity.Pronunciation = dto.Pronunciation?.Trim();
            entity.PartOfSpeech = dto.PartOfSpeech;
            entity.CEFRLevel = dto.CEFRLevel;
            entity.Description = dto.Description?.Trim();
            entity.ExampleSentence = dto.ExampleSentence?.Trim();
            entity.ImageUrl = imageUrl ?? entity.ImageUrl; // giữ lại ảnh cũ nếu không upload mới
            entity.PronunciationUrl = dto.PronunciationUrl?.Trim();

            await _uow.Vocabulary.UpdateVocabularyAsync(entity, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
    

            _logger.LogInformation("Vocabulary ID {Id} updated", id);

            return MapToAdminDto(entity);
        }

        // ============================================================================
        // DELETE
        // ============================================================================

        /// <summary>
        /// Xóa từ vựng (admin only).
        /// </summary>
        public async Task<bool> DeleteVocabularyAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting vocabulary ID {Id}", id);

            var result = await _uow.Vocabulary.DeleteVocabularyAsync(id, cancellationToken);
            if (result)
            {
                await _uow.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Vocabulary ID {Id} deleted", id);
            }

            return result;
        }

        // ============================================================================
        // PRIVATE HELPERS
        // ============================================================================

        private static VocabularyDto MapToDto(Vocabulary v) => new()
        {
            Id = v.Id,
            Word = v.Word,
            Meaning = v.Meaning,
            PartOfSpeech = v.PartOfSpeech.ToString(),
            CEFRLevel = v.CEFRLevel.ToString(),
            Description = v.Description,
            ExampleSentence = v.ExampleSentence,
            ImageUrl = v.ImageUrl,
            Pronunciation = v.Pronunciation,
            PronunciationUrl = v.PronunciationUrl
        };

        private static AdminVocabularyDto MapToAdminDto(Vocabulary v) => new()
        {
            Id = v.Id,
            Word = v.Word,
            Meaning = v.Meaning,
            Pronunciation = v.Pronunciation,
            PartOfSpeech = v.PartOfSpeech.ToString(),
            CEFRLevel = v.CEFRLevel.ToString(),
            Description = v.Description,
            ExampleSentence = v.ExampleSentence,
            ImageUrl = v.ImageUrl,
            PronunciationUrl = v.PronunciationUrl
        };
    }
}