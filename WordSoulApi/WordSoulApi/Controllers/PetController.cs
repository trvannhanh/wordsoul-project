using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WordSoulApi.Extensions;
using WordSoulApi.Models.DTOs.Pet;
using WordSoulApi.Models.Entities;
using WordSoulApi.Services.Implementations;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Controllers
{
    [Route("api/pets")]
    [ApiController]
    [EnableCors("AllowLocalhost")]
    public class PetController : ControllerBase
    {
        private readonly IPetService _petService;
        private readonly IUploadAssetsService _uploadAssetsService;
        public PetController(IPetService petService , IUploadAssetsService uploadAssetsService)
        {
            _petService = petService;
            _uploadAssetsService = uploadAssetsService;
        }

        // GET: api/pets : Lấy tất cả pet
        [Authorize(Roles = "Admin,User")]
        [HttpGet]
        public async Task<IActionResult> GetAllPets(string? name, PetRarity? rarity, PetType? type,
                                                                bool? isOwned, int pageNumber = 1, int pageSize = 10)
        {

            // Lấy userId từ token
            var userId = User.GetUserId();
            // Nếu userId không hợp lệ, trả về lỗi Unauthorized
            if (userId == 0) return Unauthorized();

            var pets = await _petService.GetAllPetsAsync(userId, name, rarity, type, isOwned, pageNumber, pageSize);
            return Ok(pets);
        }

        // GET: api/pets/{id} : Lấy pet theo ID
        [Authorize(Roles = "Admin,User")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPetById(int id)
        {
            var pet = await _petService.GetPetByIdAsync(id);
            if (pet == null) return NotFound();
            return Ok(pet);

        }

        // POST: api/pets : Tạo pet mới
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreatePet([FromForm] CreatePetDto petDto)
        {

            if (petDto == null)
            {
                return BadRequest("Pet data is required.");
            }

            try
            {
                string? imageUrl = null;
                string? publicId = null;

                Console.WriteLine($"ImageFile: {petDto.ImageFile?.FileName}, Length: {petDto.ImageFile?.Length}");
                if (petDto.ImageFile != null && petDto.ImageFile.Length > 0)
                {
                    Console.WriteLine("111111");
                    (imageUrl, publicId) = await _uploadAssetsService.UploadImageAsync(petDto.ImageFile, "pets");
                }

                // Gọi service để tạo VocabularySet
                var createdPet = await _petService.CreatePetAsync(petDto, imageUrl);

                return CreatedAtAction(nameof(GetPetById), new { id = createdPet.Id }, createdPet);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while creating the pet.", Error = ex.Message });
            }
        }

        // PUT: api/pets/{id} : Cập nhật pet theo ID
        //[Authorize(Roles = "Admin")]
        //[HttpPut("{id}")]
        //public async Task<IActionResult> UpdatePet(int id, AdminPetDto petDto)
        //{
        //    if (petDto == null) return BadRequest("Pet data is required.");
        //    var updatedPet = await _petService.UpdatePetAsync(id, petDto);
        //    if (updatedPet == null) return NotFound();
        //    return Ok(updatedPet);
        //}

        //// DELETE: api/pets/{id} : Xóa pet theo ID
        //[Authorize(Roles = "Admin")]
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeletePet(int id)
        //{
        //    var result = await _petService.DeletePetAsync(id);
        //    if (!result) return NotFound();
        //    return NoContent();
        //}


        // api lọc pet theo loại, tên, cấp độ, trạng thái


    }
}
