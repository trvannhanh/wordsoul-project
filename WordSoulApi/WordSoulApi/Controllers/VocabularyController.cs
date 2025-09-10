using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WordSoulApi.Models.DTOs.Vocabulary;
using WordSoulApi.Models.DTOs.VocabularySet;
using WordSoulApi.Models.Entities;
using WordSoulApi.Services.Implementations;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Controllers
{
    [Route("api/vocabularies")]
    [ApiController]
    [EnableCors("AllowLocalhost")]
    public class VocabularyController : ControllerBase
    {
        private readonly IVocabularyService _vocabularyService;
        private readonly IUploadAssetsService _uploadAssetsService;
        private readonly ILogger<VocabularyController> _logger;
        public VocabularyController(IVocabularyService vocabularyService, IUploadAssetsService uploadAssetsService, ILogger<VocabularyController> logger)
        {
            _vocabularyService = vocabularyService;
            _uploadAssetsService = uploadAssetsService;
            _logger = logger;
        }

        // GET: api/vocabularies : Lấy tất cả từ vựng
        [Authorize(Roles = "Admin,User")]
        [HttpGet]
        public async Task<IActionResult> GetAllVocabularies(string? word, string? meaning, PartOfSpeech? partOfSpeech, CEFRLevel? cEFRLevel, int pageNumber = 1, int pageSize = 10)
        {
            var vocabularies = await _vocabularyService.GetAllVocabulariesAsync(word, meaning, partOfSpeech, cEFRLevel, pageNumber, pageSize);
            return Ok(vocabularies);
        }

        // GET: api/vocabularies/{id} : Lấy từ vựng theo ID
        [Authorize(Roles = "Admin,User")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVocabularyById(int id)
        {
            var vocabulary = await _vocabularyService.GetVocabularyByIdAsync(id);
            if (vocabulary == null) return NotFound();
            return Ok(vocabulary);
        }

        // POST: api/vocabularies : Tạo từ vựng mới
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateVocabulary([FromForm] CreateVocabularyDto vocabularyDto)
        {
            if (vocabularyDto == null)
            {
                return BadRequest("Vocabulary data is required.");
            }

            try
            {
                string? imageUrl = null;
                string? publicId = null;

                // Upload ảnh nếu có
                if (vocabularyDto.ImageFile != null && vocabularyDto.ImageFile.Length > 0)
                {
                    // public ID dùng để xóa ảnh (rollback) sau này nếu cần
                    (imageUrl, publicId) = await _uploadAssetsService.UploadImageAsync(vocabularyDto.ImageFile, "vocabularies");
                }

                // Gọi service để tạo Vocabulary
                var createdVocabularySet = await _vocabularyService.CreateVocabularyAsync(vocabularyDto, imageUrl);

                return CreatedAtAction(nameof(GetVocabularyById), new { id = createdVocabularySet.Id }, createdVocabularySet);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while creating the vocabulary set.", Error = ex.Message });
            }
        }

        // PUT: api/vocabularies/{id} : Cập nhật từ vựng theo ID
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVocabulary(int id, [FromForm] CreateVocabularyDto vocabularyDto)
        {
            if (vocabularyDto == null)
            {
                return BadRequest("Vocabulary data is required.");
            }

            try
            {
                string? imageUrl = null;
                string? publicId = null;

                // Upload ảnh nếu có
                if (vocabularyDto.ImageFile != null && vocabularyDto.ImageFile.Length > 0)
                {
                    // public ID dùng để xóa ảnh (rollback) sau này nếu cần
                    (imageUrl, publicId) = await _uploadAssetsService.UploadImageAsync(vocabularyDto.ImageFile, "vocabularies");
                }

                // Gọi service để cập nhật Vocabulary
                var updatedVocabulary = await _vocabularyService.UpdateVocabularyAsync(id, vocabularyDto, imageUrl);
                if (updatedVocabulary == null) return NotFound();
                return Ok(updatedVocabulary);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while creating the vocabulary set.", Error = ex.Message });
            }
        }

        // DELETE: api/vocabularies/{id} : Xóa từ vựng theo ID
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVocabulary(int id)
        {
            var result = await _vocabularyService.DeleteVocabularyAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        // POST: api/vocabularies/search : Tìm kiếm từ vựng theo danh sách từ
        [Authorize(Roles = "Admin,User")]
        [HttpPost("search")]
        public async Task<IActionResult> SearchVocabularies(SearchVocabularyDto searchDto)
        {
            if (searchDto == null || searchDto.Words == null || !searchDto.Words.Any())
                return BadRequest("Vocabulary List are required");

            var vocabularies = await _vocabularyService.GetVocabulariesByWordsAsync(searchDto);
            return Ok(vocabularies);
        }

        // POST: api/vocabulary-sets/{setId}/vocabularies : Thêm từ vựng vào bộ từ vựng
        [Authorize(Roles = "Admin")]
        [HttpPost("{setId}/vocabularies")]
        public async Task<IActionResult> AddVocabularyToSet(int setId, [FromForm] CreateVocabularyInSetDto vocabularyDto)
        {
            if (vocabularyDto == null)
            {
                return BadRequest("Vocabulary data is required.");
            }

            try
            {
                string? imageUrl = null;
                string? publicId = null;

                if (vocabularyDto.ImageFile != null && vocabularyDto.ImageFile.Length > 0)
                {
                    (imageUrl, publicId) = await _uploadAssetsService.UploadImageAsync(vocabularyDto.ImageFile, "vocabularies");
                }

                var createdVocabulary = await _vocabularyService.AddVocabularyToSetAsync(setId, vocabularyDto, imageUrl);
                if (createdVocabulary == null) return NotFound("Vocabulary set not found.");

                return CreatedAtAction(nameof(GetVocabularyById), new { id = createdVocabulary.Id }, createdVocabulary);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while adding vocabulary.", Error = ex.Message });
            }
        }

        // DELETE: api/vocabulary-sets/{setId}/vocabularies/{vocabId} : Xóa từ vựng khỏi bộ từ vựng
        [Authorize(Roles = "Admin")]
        [HttpDelete("{setId}/vocabularies/{vocabId}")]
        public async Task<IActionResult> RemoveVocabularyFromSet(int setId, int vocabId)
        {
            var result = await _vocabularyService.RemoveVocabularyFromSetAsync(setId, vocabId);
            if (!result) return NotFound("Vocabulary or set not found.");
            return NoContent();
        }

        // GET: api/vocabulary-sets/{setId}/vocabularies : Lấy danh sách từ vựng trong bộ từ vựng
        [Authorize(Roles = "Admin,User")]
        [HttpGet("{setId}/vocabularies")]
        public async Task<IActionResult> GetVocabulariesInSet(int setId, int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1) return BadRequest("Invalid pagination parameters.");

            var result = await _vocabularyService.GetVocabulariesInSetAsync(setId, pageNumber, pageSize);

            return Ok(result);
        }

    }
}
    