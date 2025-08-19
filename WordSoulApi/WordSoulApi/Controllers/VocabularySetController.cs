using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WordSoulApi.Models.DTOs.VocabularySet;
using WordSoulApi.Models.Entities;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VocabularySetController : ControllerBase
    {
        private readonly IVocabularySetService _vocabularySetService;

        public VocabularySetController(IVocabularySetService vocabularySetService)
        {
            _vocabularySetService = vocabularySetService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllVocabularySets(int pageNumber = 1, int pageSize = 10)
        {
            var vocabularySets = await _vocabularySetService.GetAllVocabularySetsAsync(pageNumber, pageSize);
            return Ok(vocabularySets);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVocabularySetById(int id)
        {
            var vocabularySet = await _vocabularySetService.GetVocabularySetByIdAsync(id);
            if (vocabularySet == null)
            {
                return NotFound();
            }
            return Ok(vocabularySet);
        }

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

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
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

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteVocabularySet(int id)
        {
            var result = await _vocabularySetService.DeleteVocabularySetAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchVocabularySets(string? title, VocabularySetTheme? theme, VocabularyDifficultyLevel? difficulty,
                                                                DateTime? createdAfter, int pageNumber = 1, int pageSize = 10)
        {
            var results = await _vocabularySetService.SearchVocabularySetAsync(
                title, theme, difficulty, createdAfter, pageNumber, pageSize);

            return Ok(results);
        }



        // api lấy danh sách pet theo bộ từ vựng
    }
}
