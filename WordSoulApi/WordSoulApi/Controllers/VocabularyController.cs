using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WordSoulApi.Models.DTOs.Vocabulary;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VocabularyController : ControllerBase
    {
        private readonly IVocabularyService _vocabularyService;
        public VocabularyController(IVocabularyService vocabularyService)
        {
            _vocabularyService = vocabularyService;
        }

        // GET: api/Vocabulary : Lấy tất cả từ vựng
        [Authorize(Roles = "Admin,User")]
        [HttpGet]
        public async Task<IActionResult> GetAllVocabularies()
        {
            var vocabularies = await _vocabularyService.GetAllVocabulariesAsync();
            return Ok(vocabularies);
        }

        // GET: api/Vocabulary/{id} : Lấy từ vựng theo ID
        [Authorize(Roles = "Admin,User")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVocabularyById(int id)
        {
            var vocabulary = await _vocabularyService.GetVocabularyByIdAsync(id);
            if (vocabulary == null) return NotFound();
            return Ok(vocabulary);
        }

        // POST: api/Vocabulary : Tạo từ vựng mới
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateVocabulary(VocabularyDto vocabularyDto)
        {
            var createdVocabulary = await _vocabularyService.CreateVocabularyAsync(vocabularyDto);
            return CreatedAtAction(nameof(GetVocabularyById), new { id = createdVocabulary.Id }, createdVocabulary);
        }

        // PUT: api/Vocabulary/{id} : Cập nhật từ vựng theo ID
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVocabulary(int id, VocabularyDto vocabularyDto)
        {
            var updatedVocabulary = await _vocabularyService.UpdateVocabularyAsync(id, vocabularyDto);
            if (updatedVocabulary == null) return NotFound();
            return Ok(updatedVocabulary);
        }

        // DELETE: api/Vocabulary/{id} : Xóa từ vựng theo ID
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVocabulary(int id)
        {
            var result = await _vocabularyService.DeleteVocabularyAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        // POST: api/Vocabulary/search : Tìm kiếm từ vựng theo danh sách từ
        [Authorize(Roles = "Admin,User")]
        [HttpPost("search")]
        public async Task<IActionResult> SearchVocabularies(SearchVocabularyDto searchDto)
        {
            if (searchDto == null || searchDto.Words == null || !searchDto.Words.Any())
                return BadRequest("Vocabulary List are required");

            var vocabularies = await _vocabularyService.GetVocabulariesByWordsAsync(searchDto);
            return Ok(vocabularies);
        }



        // api lấy danh sách câu hỏi quiz theo từ vựng

        // lọc từ vựng theo chủ đề, độ khó, loại từ, v.v.
    }
}
    