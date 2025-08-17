using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WordSoulApi.Models.DTOs.QuizQuestion;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizQuestionController : ControllerBase
    {
        private readonly IQuizQuestionService _quizQuestionService;
        public QuizQuestionController(IQuizQuestionService quizQuestionService)
        {
            _quizQuestionService = quizQuestionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllQuizQuestions()
        {
            var quizQuestions = await _quizQuestionService.GetAllQuizQuestionsAsync();
            return Ok(quizQuestions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuizQuestionById(int id)
        {
            var quizQuestion = await _quizQuestionService.GetQuizQuestionByIdAsync(id);
            if (quizQuestion == null) return NotFound();
            return Ok(quizQuestion);
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuizQuestion(CreateQuizQuestionDto quizQuestionDto)
        {
            if (quizQuestionDto == null) return BadRequest("Quiz question data is required.");
            var createdQuizQuestion = await _quizQuestionService.CreateQuizQuestionAsync(quizQuestionDto);
            return CreatedAtAction(nameof(GetQuizQuestionById), new { id = createdQuizQuestion.Id }, createdQuizQuestion);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuizQuestion(int id, AdminQuizQuestionDto quizQuestionDto)
        {
            if (quizQuestionDto == null) return BadRequest("Quiz question data is required.");
            var updatedQuizQuestion = await _quizQuestionService.UpdateQuizQuestionAsync(id, quizQuestionDto);
            if (updatedQuizQuestion == null) return NotFound();
            return Ok(updatedQuizQuestion);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuizQuestion(int id)
        {
            var result = await _quizQuestionService.DeleteQuizQuestionAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
