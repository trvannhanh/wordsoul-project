using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WordSoulApi.Models.DTOs.QuizQuestion;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Controllers
{
    [Route("api/quiz-questions")]
    [ApiController]
    [EnableCors("AllowLocalhost")]
    public class QuizQuestionController : ControllerBase
    {
        private readonly IQuizQuestionService _quizQuestionService;
        public QuizQuestionController(IQuizQuestionService quizQuestionService)
        {
            _quizQuestionService = quizQuestionService;
        }

        // GET : api/quiz-questions : Lấy tất cả câu hỏi trắc nghiệm
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllQuizQuestions()
        {
            var quizQuestions = await _quizQuestionService.GetAllQuizQuestionsAsync();
            return Ok(quizQuestions);
        }

        // GET : api/quiz-questions/{id} : Lấy câu hỏi trắc nghiệm theo ID
        [Authorize(Roles = "Admin,User")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuizQuestionById(int id)
        {
            var quizQuestion = await _quizQuestionService.GetQuizQuestionByIdAsync(id);
            if (quizQuestion == null) return NotFound();
            return Ok(quizQuestion);
        }

        // POST : api/quiz-questions : Tạo câu hỏi trắc nghiệm mới
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateQuizQuestion(CreateQuizQuestionDto quizQuestionDto)
        {
            if (quizQuestionDto == null) return BadRequest("Quiz question data is required.");
            var createdQuizQuestion = await _quizQuestionService.CreateQuizQuestionAsync(quizQuestionDto);
            return CreatedAtAction(nameof(GetQuizQuestionById), new { id = createdQuizQuestion.Id }, createdQuizQuestion);
        }

        // PUT : api/quiz-questions/{id} : Cập nhật câu hỏi trắc nghiệm theo ID
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuizQuestion(int id, AdminQuizQuestionDto quizQuestionDto)
        {
            if (quizQuestionDto == null) return BadRequest("Quiz question data is required.");
            var updatedQuizQuestion = await _quizQuestionService.UpdateQuizQuestionAsync(id, quizQuestionDto);
            if (updatedQuizQuestion == null) return NotFound();
            return Ok(updatedQuizQuestion);
        }

        // DELETE : api/quiz-questions/{id} : Xóa câu hỏi trắc nghiệm theo ID
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuizQuestion(int id)
        {
            var result = await _quizQuestionService.DeleteQuizQuestionAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }


     

    }
}
