using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WordSoulApi.Extensions;
using WordSoulApi.Models.DTOs.VocabularySet;
using WordSoulApi.Models.Entities;
using WordSoulApi.Services.Implementations;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VocabularySetController : ControllerBase
    {
        private readonly IVocabularySetService _vocabularySetService;
        private readonly IUserVocabularySetService _userVocabularySetService;
        private readonly ILogger<VocabularySetController> _logger;

        public VocabularySetController(IVocabularySetService vocabularySetService, IUserVocabularySetService userVocabularySetService, ILogger<VocabularySetController> logger)
        {
            _vocabularySetService = vocabularySetService;
            _userVocabularySetService = userVocabularySetService;
            _logger = logger;
        }

        // GET: api/vocabulary-sets : Lấy tất cả bộ từ vựng với phân trang
        [Authorize(Roles = "Admin,User")]
        [HttpGet]
        public async Task<IActionResult> GetAllVocabularySets(int pageNumber = 1, int pageSize = 10)
        {
            var vocabularySets = await _vocabularySetService.GetAllVocabularySetsAsync(pageNumber, pageSize);
            return Ok(vocabularySets);
        }

        // GET: api/vocabulary-sets/{id} : Lấy bộ từ vựng theo ID
        [Authorize(Roles = "Admin,User")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVocabularySetById(int id)
        {
            if (id <= 0) return BadRequest("Invalid VocabularySet ID");

            try
            {
                var vocabularySet = await _vocabularySetService.GetVocabularySetByIdAsync(id);
                if (vocabularySet == null) return NotFound();
                return Ok(vocabularySet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vocabulary set with ID: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/vocabulary-sets : Tạo bộ từ vựng mới
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateVocabularySet([FromBody] CreateVocabularySetDto createDto)
        {
            if (createDto == null)
            {
                return BadRequest("Vocabulary set data is required.");
            }

            try
            {
                var createdVocabularySet = await _vocabularySetService.CreateVocabularySetAsync(createDto);
                return CreatedAtAction(nameof(GetVocabularySetById), new { id = createdVocabularySet.Id }, createdVocabularySet);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/vocabulary-sets/{id} : Cập nhật bộ từ vựng theo ID
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVocabularySet(int id, [FromBody] UpdateVocabularySetDto updateDto)
        {
            if (updateDto == null)
            {
                return BadRequest("Vocabulary set data is required.");
            }

            try
            {
                var updatedVocabularySet = await _vocabularySetService.UpdateVocabularySetAsync(id, updateDto);
                if (updatedVocabularySet == null)
                {
                    return NotFound();
                }
                return Ok(updatedVocabularySet);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/vocabulary-sets/{id} : Xóa bộ từ vựng theo ID
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVocabularySet(int id)
        {
            var result = await _vocabularySetService.DeleteVocabularySetAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        // GET: api/vocabulary-sets/search : Tìm kiếm bộ từ vựng với các tiêu chí khác nhau và phân trang
        [Authorize(Roles = "Admin,User")]
        [HttpGet("search")]
        public async Task<IActionResult> SearchVocabularySets(string? title, VocabularySetTheme? theme, VocabularyDifficultyLevel? difficulty,
                                                                DateTime? createdAfter, int pageNumber = 1, int pageSize = 10)
        {
            var results = await _vocabularySetService.SearchVocabularySetAsync(
                title, theme, difficulty, createdAfter, pageNumber, pageSize);

            return Ok(results);
        }

        // POST: api/vocabulary-sets/{vocabId} : Thêm bộ từ vựng vào người dùng hiện tại
        [Authorize(Roles = "User")]
        [HttpPost("{vocabId}")]
        public async Task<IActionResult> AddVocabularySet(int vocabId)
        {
            if (vocabId <= 0) return BadRequest("Invalid VocabularySet ID");

            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();

            try
            {
                await _userVocabularySetService.AddVocabularySetToUserAsync(userId, vocabId);
                return Ok(new { message = "VocabularySet added successfully", userId, vocabId });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding VocabularySet {VocabId} for User {UserId}", vocabId, userId);
                return StatusCode(500, "Internal server error");
            }
        }


        // api lấy danh sách pet theo bộ từ vựng
    }
}
