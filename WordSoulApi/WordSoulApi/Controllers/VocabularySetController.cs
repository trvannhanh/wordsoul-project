using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WordSoulApi.Extensions;
using WordSoulApi.Models.DTOs.VocabularySet;
using WordSoulApi.Models.Entities;
using WordSoulApi.Services.Implementations;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Controllers
{
    [Route("api/vocabulary-sets")]
    [ApiController]
    [EnableCors("AllowLocalhost")]
    public class VocabularySetController : ControllerBase
    {
        private readonly IVocabularySetService _vocabularySetService;
        private readonly IUserVocabularySetService _userVocabularySetService;
        private readonly IUploadAssetsService _uploadAssetsService;
        private readonly ILogger<VocabularySetController> _logger;

        public VocabularySetController(IVocabularySetService vocabularySetService, IUserVocabularySetService userVocabularySetService, ILogger<VocabularySetController> logger, IUploadAssetsService uploadAssetsService)
        {
            _vocabularySetService = vocabularySetService;
            _userVocabularySetService = userVocabularySetService;
            _logger = logger;
            _uploadAssetsService = uploadAssetsService;
        }

        // GET: api/vocabulary-sets/{id} : Lấy bộ từ vựng theo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<VocabularySetDetailDto>> GetVocabularySetById(int id)
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

        // GET: api/vocabulary-sets/{id}/details : Lấy bộ từ vựng theo ID kèm chi tiết các từ vựng bên trong với phân trang
        [HttpGet("{id}/details")]
        public async Task<ActionResult<VocabularySetFullDetailDto>> GetVocabularySetFullDetails(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (id <= 0) return BadRequest("Invalid VocabularySet ID");
            if (page < 1 || pageSize < 1) return BadRequest("Invalid pagination parameters");

            try
            {
                var vocabularySet = await _vocabularySetService.GetVocabularySetFullDetailsAsync(id, page, pageSize);
                if (vocabularySet == null) return NotFound();
                return Ok(vocabularySet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving full details for vocabulary set with ID: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/vocabulary-sets : Tạo bộ từ vựng mới
        [HttpPost]
        [Authorize(Roles = "Admin,User")] // Cập nhật để cho phép cả User
        public async Task<IActionResult> CreateVocabularySet([FromForm] CreateVocabularySetDto createDto)
        {
            if (createDto == null)
            {
                return BadRequest("Vocabulary set data is required.");
            }

            try
            {
                var userId = User.GetUserId();  
                if (userId == 0) return Unauthorized();

                string? imageUrl = null;
                string? publicId = null;

                // Upload ảnh nếu có
                if (createDto.ImageFile != null && createDto.ImageFile.Length > 0)
                {
                    (imageUrl, publicId) = await _uploadAssetsService.UploadImageAsync(createDto.ImageFile, "vocabulary_sets");
                }

                // Gọi service để tạo VocabularySet, truyền thêm userId
                var createdVocabularySet = await _vocabularySetService.CreateVocabularySetAsync(createDto, imageUrl, userId);

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
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while creating the vocabulary set.", Error = ex.Message });
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

        // GET: api/vocabulary-sets : Tìm kiếm bộ từ vựng với các tiêu chí khác nhau và phân trang
        [HttpGet]
        public async Task<IActionResult> GetAllVocabularySets(
            string? title,
            VocabularySetTheme? theme,
            VocabularyDifficultyLevel? difficulty,
            DateTime? createdAfter,
            bool? isOwned, // Bộ lọc sở hữu (chỉ áp dụng khi đăng nhập)
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                int? userId = User.GetUserId(); // Lấy userId từ JWT, trả về 0 nếu chưa đăng nhập

                // Nếu isOwned được yêu cầu nhưng chưa đăng nhập, trả lỗi
                if (isOwned.HasValue && userId == 0)
                {
                    return BadRequest("Cannot filter by ownership without logging in.");
                }

                var results = await _vocabularySetService.GetAllVocabularySetsAsync(
                    title, theme, difficulty, createdAfter, isOwned, userId, pageNumber, pageSize);

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vocabulary sets");
                return StatusCode(500, new { Message = "An error occurred while retrieving vocabulary sets.", Error = ex.Message });
            }
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
