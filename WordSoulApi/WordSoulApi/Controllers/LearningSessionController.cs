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
            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();
            if (vocaSetId <= 0) return BadRequest("Invalid VocabularySet ID");

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

        // POST: api/learning-sessions/ : Tạo một phiên học ôn tập mới cho người dùng hiện tại 
        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> CreateReviewingSession()
        {
            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();

            try
            {
                var learningSessionDto = await _learningSessionService.CreateReviewingSessionAsync(userId);
                return Ok(learningSessionDto);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "No unreviewed vocabularies for user {UserId}", userId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reviewing session for user {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/learning-sessions/{sessionId}/questions : Lấy danh sách câu hỏi cho phiên học cụ thể
        [Authorize(Roles = "User")]
        [HttpGet("{sessionId}/questions")]
        public async Task<ActionResult<IEnumerable<QuizQuestionDto>>> GetSessionQuestions(int sessionId, [FromQuery] bool includeRetries = false)
        {
            try
            {
                var questions = await _learningSessionService.GetSessionQuestionsAsync(sessionId, includeRetries);
                // Luôn trả về Ok, ngay cả khi mảng rỗng (không dùng NotFound nếu không có dữ liệu)
                return Ok(questions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving questions for session {SessionId}, includeRetries: {IncludeRetries}", sessionId, includeRetries);
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

        //POST: api/learning-sessions/{sessionId}/learning-completion : Hoàn thành phiên học (cập nhật trạng thái LearningSession, cấp phần thưởng)
        [Authorize(Roles = "User")]
        [HttpPost("{sessionId}/learning-completion")]
        public async Task<IActionResult> CompleteLearningSession(int sessionId)
        {
            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();

            try
            {
                var response = await _learningSessionService.CompleteSessionAsync(userId, sessionId, SessionType.Learning);
                return Ok(response as CompleteLearningSessionResponseDto);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "User {UserId} does not have access to session {SessionId}", userId, sessionId);
                return Unauthorized(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error completing learning session for user {UserId}, session {SessionId}", userId, sessionId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing learning session for user {UserId}, session {SessionId}", userId, sessionId);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "User")]
        [HttpPost("{sessionId}/review-completion")]
        public async Task<IActionResult> CompleteReviewingSession(int sessionId)
        {
            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();

            try
            {
                var response = await _learningSessionService.CompleteSessionAsync(userId, sessionId, SessionType.Review);
                return Ok(response as CompleteReviewingSessionResponseDto);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "User {UserId} does not have access to session {SessionId}", userId, sessionId);
                return Unauthorized(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error completing reviewing session for user {UserId}, session {SessionId}", userId, sessionId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing reviewing session for user {UserId}, session {SessionId}", userId, sessionId);
                return StatusCode(500, "Internal server error");
            }
        }
    } 
}
