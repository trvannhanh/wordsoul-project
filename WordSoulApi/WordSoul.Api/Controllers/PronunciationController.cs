using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using WordSoul.Api.Extensions;
using WordSoul.Application.DTOs.Pronunciation;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Application.Interfaces.Services;

namespace WordSoulApi.Controllers
{
    [Route("api/pronunciation-attempts")]
    [ApiController]
    [EnableCors("AllowLocalhost")]
    [Authorize(Roles = "User")]
    public class PronunciationController : ControllerBase
    {
        private readonly IPronunciationPracticeService _pronunciationPracticeService;

        public PronunciationController(IPronunciationPracticeService pronunciationPracticeService)
        {
            _pronunciationPracticeService = pronunciationPracticeService;
        }

        /// <summary>
        /// GET /api/pronunciation-attempts/practice-vocabularies?limit=10
        /// Lấy danh sách từ vựng đã học cần được luyện phát âm
        /// </summary>
        [HttpGet("practice-vocabularies")]
        public async Task<ActionResult<List<PronunciationWordDto>>> GetPracticeVocabularies(
            [FromQuery] [Range(1, 50)] int limit = 10,
            CancellationToken ct = default)
        {
            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();

            var wordsForPractice = await _pronunciationPracticeService.GetWordsForPracticeAsync(userId, limit, ct);
            return Ok(wordsForPractice);
        }

        /// <summary>
        /// POST /api/pronunciation-attempts
        /// Gửi file ghi âm để chấm điểm phát âm và ghi nhận kết quả
        /// </summary>
        [HttpPost]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10MB max audio
        public async Task<ActionResult<PronunciationAssessResponse>> CreateAttempt(
            [FromForm] PronunciationAssessRequest request,
            CancellationToken ct)
        {
            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();

            using (var audioStream = request.Audio.OpenReadStream())
            {
                var response = await _pronunciationPracticeService.AssessPronunciationAsync(
                    userId,
                    request.VocabularyId,
                    request.Word,
                    audioStream,
                    request.Audio.ContentType ?? "",
                    request.PetId,
                    ct);

                return Ok(response);
            }
        }

        /// <summary>
        /// GET /api/pronunciation-attempts/vocabularies/{vocabularyId:int}
        /// Lấy lịch sử các lượt phát âm của từ vựng cụ thể
        /// </summary>
        [HttpGet("vocabularies/{vocabularyId:int}")]
        public async Task<ActionResult<List<PronunciationHistoryDto>>> GetHistory(
            [Range(1, int.MaxValue)] int vocabularyId, 
            CancellationToken ct)
        {
            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();

            var history = await _pronunciationPracticeService.GetHistoryAsync(userId, vocabularyId, ct);
            return Ok(history);
        }

        /// <summary>
        /// GET /api/pronunciation-attempts/stats
        /// Lấy số liệu thống kê tổng hợp kết quả phát âm của user
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<PronunciationStatsDto>> GetStats(CancellationToken ct)
        {
            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();

            var stats = await _pronunciationPracticeService.GetStatsAsync(userId, ct);
            return Ok(stats);
        }
    }

    // ── DTOs ────────────────────────────────────────────────────────────────

    public class PronunciationAssessRequest
    {
        [Required(ErrorMessage = "Audio file is required.")]
        public required IFormFile Audio { get; set; }

        [Required(ErrorMessage = "VocabularyId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "VocabularyId must be greater than 0.")]
        public int VocabularyId { get; set; }

        [Required(ErrorMessage = "Reference word is required.")]
        public required string Word { get; set; }

        public int? PetId { get; set; }
    }
}
