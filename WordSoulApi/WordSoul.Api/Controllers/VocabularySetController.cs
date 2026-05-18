using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using WordSoul.Api.Extensions;
using WordSoul.Application.DTOs.Vocabulary;
using WordSoul.Application.DTOs.VocabularySet;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Enums;

namespace WordSoul.Api.Controllers
{
    [Route("api/vocabulary-sets")]
    [ApiController]
    [EnableCors("AllowFrontend")]
    public class VocabularySetController : ControllerBase
    {
        private readonly IVocabularySetService _vocabularySetService;
        private readonly ISetVocabularyService _setVocabularyService;
        private readonly IUserVocabularySetService _userVocabularySetService;
        private readonly IUploadAssetsService _uploadAssetsService;
        private readonly ILogger<VocabularySetController> _logger;

        public VocabularySetController(IVocabularySetService vocabularySetService, IUserVocabularySetService userVocabularySetService, ILogger<VocabularySetController> logger, IUploadAssetsService uploadAssetsService, ISetVocabularyService setVocabularyService)
        {
            _vocabularySetService = vocabularySetService;
            _userVocabularySetService = userVocabularySetService;
            _logger = logger;
            _uploadAssetsService = uploadAssetsService;
            _setVocabularyService = setVocabularyService;
        }

        //------------------------------ POST -----------------------------------------

        // POST: api/vocabulary-sets : Tạo bộ từ vựng mới
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,User")]
        public async Task<IActionResult> CreateVocabularySet([FromForm] CreateVocabularySetDto createDto)
        {
            if (createDto == null)
            {
                _logger.LogWarning("CreateVocabularySet failed: DTO is null.");
                return BadRequest("Vocabulary set data is required.");
            }

            // Validation thêm
            if (createDto.VocabularyIds == null || !createDto.VocabularyIds.Any())
            {
                _logger.LogWarning("VocabularyIds is empty or null.");
                return BadRequest("At least one vocabulary ID is required.");
            }

            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();

            try
            {
                string? imageUrl = null;
                string? publicId = null;

                // Upload ảnh với validation
                if (createDto.ImageFile != null && createDto.ImageFile.Length > 0)
                {
                    if (createDto.ImageFile.Length > 10 * 1024 * 1024) // 10MB
                    {
                        _logger.LogWarning("Image file too large for user {UserId}", userId);
                        return BadRequest("Image file size exceeds 10MB.");
                    }
                    if (!createDto.ImageFile.ContentType.StartsWith("image/"))
                    {
                        _logger.LogWarning("Invalid image type for user {UserId}", userId);
                        return BadRequest("Only image files are allowed.");
                    }
                    (imageUrl, publicId) = await _uploadAssetsService.UploadImageAsync(createDto.ImageFile, "vocabulary_sets");
                }

                var createdVocabularySet = await _vocabularySetService.CreateVocabularySetAsync(createDto, imageUrl, userId);
                _logger.LogInformation("Created vocabulary set ID: {Id} by user {UserId}", createdVocabularySet.Id, userId);
                return CreatedAtAction(nameof(GetVocabularySetById), new { id = createdVocabularySet.Id }, createdVocabularySet);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Validation error in CreateVocabularySet for user {UserId}: {Message}", userId, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Key not found in CreateVocabularySet for user {UserId}: {Message}", userId, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal error in CreateVocabularySet for user {UserId}", userId);
                // Rollback ảnh nếu có
                //if (!string.IsNullOrEmpty(publicId))
                //{
                //    await _uploadAssetsService.DeleteImageAsync(publicId);
                //    _logger.LogDebug("Rolled back uploaded image {PublicId} for user {UserId}", publicId, userId);
                //}
                return StatusCode(500, new { Message = "An error occurred while creating the vocabulary set.", Error = ex.Message });
            }
        }

        // POST: api/vocabulary-sets/ai-preview : Xem trước dữ liệu tạo từ vựng
        [HttpPost("ai-preview")]
        [Authorize(Roles = "Admin,SuperAdmin,User")]
        public async Task<IActionResult> AiPreviewVocabularySet([FromBody] AiPreviewRequestDto dto)
        {
            if (dto == null || dto.Words == null || !dto.Words.Any())
            {
                return BadRequest("At least one word is required.");
            }

            if (dto.Words.Count > 50)
            {
                return BadRequest("Maximum 50 words per request.");
            }

            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();

            try
            {
                var result = await _vocabularySetService.AiPreviewVocabularySetAsync(dto, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AiPreviewVocabularySet for user {UserId}", userId);
                return StatusCode(500, "An error occurred while generating vocabulary preview.");
            }
        }

        // POST: api/vocabulary-sets/ai-create : Tạo bộ từ vựng mới với AI hỗ trợ
        [HttpPost("ai-create")]
        [Authorize(Roles = "Admin,SuperAdmin,User")]
        public async Task<IActionResult> AiCreateVocabularySet([FromForm] AiCreateVocabularySetDto createDto)
        {
            if (createDto == null)
            {
                _logger.LogWarning("AiCreateVocabularySet failed: DTO is null.");
                return BadRequest("Vocabulary set data is required.");
            }

            if (createDto.Vocabularies == null || !createDto.Vocabularies.Any())
            {
                _logger.LogWarning("AiCreateVocabularySet failed: Vocabularies list is empty.");
                return BadRequest("At least one word is required.");
            }

            if (createDto.Vocabularies.Count > 50)
            {
                return BadRequest("Maximum 50 words per request.");
            }

            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();

            try
            {
                string? imageUrl = null;

                if (createDto.ImageFile != null && createDto.ImageFile.Length > 0)
                {
                    if (createDto.ImageFile.Length > 10 * 1024 * 1024)
                        return BadRequest("Image file size exceeds 10MB.");
                    if (!createDto.ImageFile.ContentType.StartsWith("image/"))
                        return BadRequest("Only image files are allowed.");

                    (imageUrl, _) = await _uploadAssetsService.UploadImageAsync(createDto.ImageFile, "vocabulary_sets");
                }

                _logger.LogInformation("User {UserId} starting AI-create for {Count} words", userId, createDto.Vocabularies.Count);

                var result = await _vocabularySetService.AiCreateVocabularySetAsync(createDto, imageUrl, userId);

                _logger.LogInformation(
                    "AI-create done: Set={SetId}, New={New}, Existed={Existed}, Failed={Failed}",
                    result.VocabularySet.Id, result.NewlyCreated, result.AlreadyExisted, result.FailedWords.Count);

                return CreatedAtAction(nameof(GetVocabularySetById), new { id = result.VocabularySet.Id }, result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Validation error in AiCreateVocabularySet for user {UserId}", userId);
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Operation error in AiCreateVocabularySet for user {UserId}", userId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal error in AiCreateVocabularySet for user {UserId}", userId);
                return StatusCode(500, new { Message = "An error occurred during AI vocabulary set creation.", Error = ex.Message });
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
                _logger.LogInformation("Added VocabularySetTo User: {Id} by user {UserId}", vocabId, userId);
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

        // DELETE: api/vocabulary-sets/{id}/user : Hủy đăng ký bộ từ vựng
        [Authorize(Roles = "User")]
        [HttpDelete("{id}/user")]
        public async Task<IActionResult> UnregisterVocabularySet(int id)
        {
            if (id <= 0) return BadRequest("Invalid VocabularySet ID");

            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();

            try
            {
                var removed = await _userVocabularySetService.RemoveVocabularySetFromUserAsync(userId, id);
                if (!removed) return NotFound("Bạn chưa đăng ký bộ từ vựng này.");
                _logger.LogInformation("User {UserId} unregistered VocabularySet {SetId}", userId, id);
                return Ok(new { message = "Hủy đăng ký thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering VocabularySet {SetId} for User {UserId}", id, userId);
                return StatusCode(500, "Internal server error");
            }
        }

        //------------------------------ GET -----------------------------------------

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
                var vocabularySet = await _setVocabularyService.GetVocabularySetFullDetailsAsync(id, page, pageSize);
                if (vocabularySet == null) return NotFound();
                return Ok(vocabularySet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving full details for vocabulary set with ID: {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/vocabulary-sets/grouped : Lấy tất cả bộ từ vựng gom nhóm theo chủ đề (giảm N+1 query frontend)
        [HttpGet("grouped")]
        public async Task<IActionResult> GetGroupedVocabularySets([FromQuery] string? title, [FromQuery] int limitPerTheme = 6)
        {
            try
            {
                int? userId = User.GetUserId(); // Có thể là 0 nếu chưa đăng nhập
                if (userId == 0) userId = null;

                var results = await _vocabularySetService.GetGroupedVocabularySetsAsync(
                    title, userId, limitPerTheme);

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving grouped vocabulary sets");
                return StatusCode(500, new { Message = "An error occurred while retrieving grouped vocabulary sets.", Error = ex.Message });
            }
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




        //------------------------------ PUT -----------------------------------------

        // PUT: api/vocabulary-sets/{id} : Cập nhật bộ từ vựng theo ID
        // Admin/SuperAdmin: không kiểm tra owner
        // User: chỉ cập nhật bộ của chính mình (validate owner trong service)
        [Authorize(Roles = "Admin,SuperAdmin,User")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVocabularySet(int id, [FromBody] UpdateVocabularySetDto updateDto)
        {
            if (updateDto == null) return BadRequest("Vocabulary set data is required.");

            try
            {
                // Admin/SuperAdmin không kiểm tra owner (null), User phải là owner
                int? requestingUserId = User.IsInRole("Admin") || User.IsInRole("SuperAdmin")
                    ? null
                    : User.GetUserId();

                var updatedVocabularySet = await _vocabularySetService.UpdateVocabularySetAsync(id, updateDto, requestingUserId);
                if (updatedVocabularySet == null) return NotFound();
                return Ok(updatedVocabularySet);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // PUT: api/vocabulary-sets/{id}/publish : Chuyển bộ private -> public
        [Authorize(Roles = "User")]
        [HttpPut("{id}/publish")]
        public async Task<IActionResult> PublishVocabularySet(int id)
        {
            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();

            try
            {
                var result = await _vocabularySetService.PublishVocabularySetAsync(id, userId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // PUT: api/vocabulary-sets/{id}/vocabularies/{vocabId} : Override nghĩa/ví dụ/phát âm từ trong bộ
        [Authorize(Roles = "User")]
        [HttpPut("{id}/vocabularies/{vocabId}")]
        public async Task<IActionResult> UpdateVocabularyInSet(
            int id, int vocabId,
            [FromBody] UpdateVocabularyInSetDto dto)
        {
            if (dto == null) return BadRequest("Override data is required.");

            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();

            try
            {
                var result = await _setVocabularyService.UpdateVocabularyInSetAsync(id, vocabId, dto, userId);
                if (result == null) return NotFound("Vocabulary not found in this set.");
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        //------------------------------ DELETE -----------------------------------------

        // DELETE: api/vocabulary-sets/{id} : Xóa bộ từ vựng theo ID
        // Admin/SuperAdmin: không kiểm tra owner
        // User: chỉ xóa bộ từ vựng của chính mình
        [Authorize(Roles = "Admin,SuperAdmin,User")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVocabularySet(int id)
        {
            try
            {
                int? requestingUserId = User.IsInRole("Admin") || User.IsInRole("SuperAdmin")
                    ? null
                    : User.GetUserId();

                var result = await _vocabularySetService.DeleteVocabularySetAsync(id, requestingUserId);
                if (!result) return NotFound();
                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        //------------------------------ GET (Progress) -----------------------------------------

        // GET: api/vocabulary-sets/{id}/my-progress : Thống kê tiến trình học của user hiện tại
        [Authorize(Roles = "User")]
        [HttpGet("{id}/my-progress")]
        public async Task<IActionResult> GetMyProgress(int id)
        {
            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();

            try
            {
                var result = await _setVocabularyService.GetVocabularySetProgressAsync(id, userId);
                if (result == null)
                    return NotFound("No progress found for this vocabulary set.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving progress for set {SetId}, user {UserId}", id, userId);
                return StatusCode(500, "An error occurred while retrieving progress.");
            }
        }

        // POST: api/vocabulary-sets/{setId}/vocabularies/new : Tạo từ vựng mới + thêm vào bộ (owner only)
        [Authorize(Roles = "Admin,SuperAdmin,User")]
        [HttpPost("{setId}/vocabularies/new")]
        public async Task<IActionResult> AddNewVocabularyToSet(int setId, [FromBody] VocabularyPreviewDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Word))
                return BadRequest("Từ vựng không được để trống.");

            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();

            try
            {
                var result = await _vocabularySetService.AddNewVocabularyToSetAsync(setId, dto, userId);
                return CreatedAtAction(nameof(GetVocabularySetById), new { id = setId }, result);
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding new vocab to set {SetId}", setId);
                return StatusCode(500, ex.Message);
            }
        }

        // PATCH: api/vocabulary-sets/{setId}/vocabularies/{vocabId}/core : Cập nhật core fields của từ vựng (owner only, custom vocab only)
        [Authorize(Roles = "Admin,SuperAdmin,User")]
        [HttpPatch("{setId}/vocabularies/{vocabId}/core")]
        public async Task<IActionResult> UpdateVocabularyCore(int setId, int vocabId, [FromForm] UpdateVocabularyCoreDto dto, IFormFile? imageFile)
        {
            if (dto == null) return BadRequest("Dữ liệu không hợp lệ.");

            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();

            try
            {
                // Upload ảnh lên Cloudinary nếu có
                if (imageFile != null && imageFile.Length > 0)
                {
                    var (uploadedUrl, _) = await _uploadAssetsService.UploadImageAsync(imageFile, "vocabularies");
                    dto.ImageUrl = uploadedUrl;
                }

                var result = await _vocabularySetService.UpdateVocabularyCoreAsync(setId, vocabId, dto, userId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vocab core {VocabId} in set {SetId}", vocabId, setId);
                return StatusCode(500, ex.Message);
            }
        }

        // POST: api/vocabulary-sets/{setId}/vocabularies/{vocabId} : Thêm từ vựng đã có vào bộ (owner only)
        [Authorize(Roles = "User")]
        [HttpPost("{setId}/vocabularies/{vocabId}")]
        public async Task<IActionResult> AddExistingVocabularyToSet(int setId, int vocabId)
        {
            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();

            try
            {
                await _setVocabularyService.AddExistingVocabularyToSetAsync(setId, vocabId, userId);
                return Ok(new { message = "Thêm từ vựng thành công." });
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (ArgumentException ex) { return Conflict(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding vocab {VocabId} to set {SetId}", vocabId, setId);
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/vocabulary-sets/{setId}/vocabularies/{vocabId} : Gỡ từ vựng khỏi bộ (owner only)
        [Authorize(Roles = "User")]
        [HttpDelete("{setId}/vocabularies/{vocabId}")]
        public async Task<IActionResult> RemoveVocabularyFromSet(int setId, int vocabId)
        {
            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();

            try
            {
                // Validate ownership before delegating to service
                var set = await _vocabularySetService.GetVocabularySetByIdAsync(setId);
                if (set == null) return NotFound("Bộ từ vựng không tồn tại.");
                if (set.CreatedById != userId) return Forbid();

                var removed = await _setVocabularyService.RemoveVocabularyFromSetAsync(setId, vocabId);
                if (!removed) return NotFound("Từ vựng không có trong bộ này.");
                return Ok(new { message = "Gỡ từ vựng thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing vocab {VocabId} from set {SetId}", vocabId, setId);
                return StatusCode(500, "Internal server error");
            }
        }

        
    }
}
