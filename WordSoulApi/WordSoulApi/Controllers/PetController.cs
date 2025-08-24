using Microsoft.AspNetCore.Authorization;
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

        // GET: api/Pet : Lấy tất cả pet
        [Authorize(Roles = "Admin,User")]
        [HttpGet]
        public async Task<IActionResult> GetAllPets()
        {
            var pets = await _petService.GetAllPetsAsync();
            return Ok(pets);
        }

        // GET: api/Pet/{id} : Lấy pet theo ID
        [Authorize(Roles = "Admin,User")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPetById(int id)
        {
            var pet = await _petService.GetPetByIdAsync(id);
            if (pet == null) return NotFound();
            return Ok(pet);
        }

        // POST: api/Pet : Tạo pet mới
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreatePet(CreatePetDto petDto)
        {
            if (petDto == null) return BadRequest("Pet data is required.");
            var createdPet = await _petService.CreatePetAsync(petDto);
            return CreatedAtAction(nameof(GetPetById), new { id = createdPet.Id }, createdPet);
        }

        // PUT: api/Pet/{id} : Cập nhật pet theo ID
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePet(int id, AdminPetDto petDto)
        {
            if (petDto == null) return BadRequest("Pet data is required.");
            var updatedPet = await _petService.UpdatePetAsync(id, petDto);
            if (updatedPet == null) return NotFound();
            return Ok(updatedPet);
        }

        // DELETE: api/Pet/{id} : Xóa pet theo ID
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePet(int id)
        {
            var result = await _petService.DeletePetAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }


        // api lọc pet theo loại, tên, cấp độ, trạng thái


    }
}
