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

        // chỉ dành cho admin
        [HttpGet]
        public async Task<IActionResult> GetAllVocabularies()
        {
            var vocabularies = await _vocabularyService.GetAllVocabulariesAsync();
            return Ok(vocabularies);
        }

        // Authorized
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVocabularyById(int id)
        {
            var vocabulary = await _vocabularyService.GetVocabularyByIdAsync(id);
            if (vocabulary == null) return NotFound();
            return Ok(vocabulary);
        }

        [HttpPost]
        public async Task<IActionResult> CreateVocabulary(VocabularyDto vocabularyDto)
        {
            var createdVocabulary = await _vocabularyService.CreateVocabularyAsync(vocabularyDto);
            return CreatedAtAction(nameof(GetVocabularyById), new { id = createdVocabulary.Id }, createdVocabulary);
        } 

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVocabulary(int id, VocabularyDto vocabularyDto)
        {
            var updatedVocabulary = await _vocabularyService.UpdateVocabularyAsync(id, vocabularyDto);
            if (updatedVocabulary == null) return NotFound();
            return Ok(updatedVocabulary);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVocabulary(int id)
        {
            var result = await _vocabularyService.DeleteVocabularyAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

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
    