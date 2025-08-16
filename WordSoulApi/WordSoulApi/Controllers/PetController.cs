using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WordSoulApi.Models.DTOs.Pet;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PetController : ControllerBase
    {
        private readonly IPetService _petService;
        public PetController(IPetService petService)
        {
            _petService = petService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPets()
        {
            var pets = await _petService.GetAllPetsAsync();
            return Ok(pets);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPetById(int id)
        {
            var pet = await _petService.GetPetByIdAsync(id);
            if (pet == null) return NotFound();
            return Ok(pet);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePet(CreatePetDto petDto)
        {
            if (petDto == null) return BadRequest("Pet data is required.");
            var createdPet = await _petService.CreatePetAsync(petDto);
            return CreatedAtAction(nameof(GetPetById), new { id = createdPet.Id }, createdPet);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePet(int id, AdminPetDto petDto)
        {
            if (petDto == null) return BadRequest("Pet data is required.");
            var updatedPet = await _petService.UpdatePetAsync(id, petDto);
            if (updatedPet == null) return NotFound();
            return Ok(updatedPet);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePet(int id)
        {
            var result = await _petService.DeletePetAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
