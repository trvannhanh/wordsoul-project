using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WordSoulApi.Extensions;
using WordSoulApi.Models.DTOs.AnswerRecord;
using WordSoulApi.Models.DTOs.LearningSession;
using WordSoulApi.Models.DTOs.QuizQuestion;
using WordSoulApi.Models.DTOs.UserVocabularyProgress;
using WordSoulApi.Models.Entities;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Controllers
{
    [Route("api/learning-sessions")]
    [ApiController]
    [EnableCors("AllowLocalhost")]
    public class LearningSessionController : ControllerBase
    {
        private readonly ILearningSessionService _learningSessionService;
        private readonly IUserVocabularyProgressService _progressService;
        private readonly ILogger<LearningSessionController> _logger;

        public LearningSessionController(ILearningSessionService learningSessionService, IUserVocabularyProgressService progressService, ILogger<LearningSessionController> logger)
        {
            _learningSessionService = learningSessionService;
            _progressService = progressService;
            _logger = logger;
        }

        // POST: api/learning-sessions/{vocaSetId} : Tạo một phiên học mới cho người dùng hiện tại dựa trên bộ từ vựng đã chọn
        [Authorize(Roles = "User")]
        [HttpPost("{vocaSetId}")]
        public async Task<IActionResult> CreateLearningSession(int vocaSetId)
        {
            // Lấy userId từ token
            var userId = User.GetUserId();
            // Nếu userId không hợp lệ, trả về lỗi Unauthorized
            if (userId == 0) return Unauthorized();

            // Kiểm tra vocaSetId hợp lệ
            if (vocaSetId <= 0) return BadRequest("Invalid VocabularySet ID");

            // Tạo phiên học mới
            try
            {
                var learningSessionDto = await _learningSessionService.CreateLearningSessionAsync(userId, vocaSetId);
                return Ok(learningSessionDto);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "No unlearned vocabularies for user {UserId} and set {SetId}", userId, vocaSetId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating learning session for user {UserId} and set {SetId}", userId, vocaSetId);
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/learning-sessions/{sessionId}/questions : Lấy danh sách câu hỏi cho phiên học cụ thể
        [Authorize(Roles = "User")]
        [HttpGet("{sessionId}/questions")]
        public async Task<ActionResult<IEnumerable<QuizQuestionDto>>> GetSessionQuestions(int sessionId)
        {
            try
            {
                var questions = await _learningSessionService.GetSessionQuestionsAsync(sessionId);
                if (!questions.Any()) return NotFound(new { message = "No questions found for this session." });
                return Ok(questions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving questions for session {SessionId}", sessionId);
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/learning-sessions/{sessionId}/answers : Gửi câu trả lời cho một câu hỏi trong phiên học
        [Authorize(Roles = "User")]
        [HttpPost("{sessionId}/answers")]
        public async Task<ActionResult<SubmitAnswerResponseDto>> SubmitAnswer(int sessionId, [FromBody] SubmitAnswerRequestDto request)
        {
            // Lấy userId từ token
            var userId = User.GetUserId();
            // Nếu userId không hợp lệ, trả về lỗi Unauthorized
            if (userId == 0) return Unauthorized();

            // Kiểm tra request hợp lệ
            try
            {
                var result = await _learningSessionService.SubmitAnswerAsync(userId, sessionId, request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access for user {UserId}, session {SessionId}", userId, sessionId);
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Question not found for user {UserId}, session {SessionId}, vocabulary {VocabularyId}", userId, sessionId, request.VocabularyId);
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request data for user {UserId}, session {SessionId}", userId, sessionId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal server error for user {UserId}, session {SessionId}, vocabulary {VocabularyId}", userId, sessionId, request.VocabularyId);
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/learning-sessions/{sessionId}/progress/{vocabId} : Cập nhật tiến trình học tập của người dùng cho từ vựng cụ thể trong phiên học
        [Authorize(Roles = "User")]
        [HttpPost("{sessionId}/progress/{vocabId}")]
        public async Task<ActionResult<UpdateProgressResponseDto>> UpdateProgress(int sessionId, int vocabId)
        {
            if (sessionId <= 0 || vocabId <= 0)
                return BadRequest("Invalid sessionId or vocabId");

            var userId = User.GetUserId();
            if (userId == 0)
                return Unauthorized();

            try
            {
                var result = await _progressService.UpdateProgressAsync(userId, sessionId, vocabId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access for user {UserId}, session {SessionId}", userId, sessionId);
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Progress update failed for user {UserId}, session {SessionId}, vocab {VocabId}", userId, sessionId, vocabId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal server error for user {UserId}, session {SessionId}, vocab {VocabId}", userId, sessionId, vocabId);
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/learning-sessions/{sessionId}/complete : Hoàn thành phiên học và cập nhật trạng thái của tất cả từ vựng trong phiên học
        [Authorize(Roles = "User")]
        [HttpPost("{sessionId}/complete")]
        public async Task<ActionResult<CompleteSessionResponseDto>> CompleteSession(int sessionId)
        {
            var userId = User.GetUserId();
            if (userId == 0)
                return Unauthorized();

            try
            {
                var result = await _learningSessionService.CompleteSessionAsync(userId, sessionId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access for user {UserId}, session {SessionId}", userId, sessionId);
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Session completion failed for user {UserId}, session {SessionId}", userId, sessionId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal server error for user {UserId}, session {SessionId}", userId, sessionId);
                return StatusCode(500, "Internal server error");
            }
        }
    } 
}
