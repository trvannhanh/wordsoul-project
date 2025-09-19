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
        private readonly IUserOwnedPetService _userOwnedPetService;
        private readonly IUploadAssetsService _uploadAssetsService;
        public PetController(IPetService petService , IUploadAssetsService uploadAssetsService, IUserOwnedPetService userOwnedPetService)
        {
            _petService = petService;
            _uploadAssetsService = uploadAssetsService;
            _userOwnedPetService = userOwnedPetService;
        }


        //------------------------------ POST -----------------------------------

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
                    (imageUrl, publicId) = await _uploadAssetsService.UploadImageAsync(petDto.ImageFile, "pets");
                }

                // Gọi service để tạo Pet
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

        // POST: api/pets/bulk : Tạo nhiều pet
        [Authorize(Roles = "Admin")]
        [HttpPost("bulk")]
        public async Task<IActionResult> CreatePetsBulk([FromBody] BulkCreatePetDto bulkDto)
        {
            if (bulkDto == null || !bulkDto.Pets.Any()) return BadRequest("Danh sách pet rỗng.");
            var createdPets = await _petService.CreatePetsBulkAsync(bulkDto);
            return Ok(createdPets);
        }


        // POST: api/pets/{petId}/evolve : Evolve pet cho user
        [Authorize(Roles = "User")]
        [HttpPost("{petId}/upgrade")]
        public async Task<IActionResult> UpgradePet(int petId)
        {
            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();

            try
            {
                var result = await _userOwnedPetService.UpgradePetForUserAsync(userId, petId);
                if (result == null) return BadRequest("Upgrade failed. Check if the pet is owned or max level reached.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while upgrading the pet.", Error = ex.Message });
            }

        }

        //------------------------------ GET -----------------------------------
        // GET: api/pets : Lấy tất cả pet theo người dùng
        [Authorize(Roles = "Admin,User")]
        [HttpGet]
        public async Task<IActionResult> GetAllPets(string? name, PetRarity? rarity, PetType? type,
                                                                bool? isOwned, int pageNumber = 1, int pageSize = 20)
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

        // GET: api/pets/{id}/detail : Lấy chi tiết pet cho người dùng
        [Authorize(Roles = "User")]
        [HttpGet("{petId}/details")]
        public async Task<IActionResult> GetPetDetail(int petId)
        {
            // Lấy userId từ token
            var userId = User.GetUserId();
            // Nếu userId không hợp lệ, trả về lỗi Unauthorized
            if (userId == 0) return Unauthorized();

            var pet = await _petService.GetPetDetailAsync(userId, petId);
            if (pet == null) return NotFound();
            return Ok(pet);

        }


        //------------------------------ PUT -----------------------------------

        //PUT: api/pets/{id} : Cập nhật pet theo ID
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePet(int id, [FromForm] UpdatePetDto petDto)
        {
            if (petDto == null) return BadRequest("Pet data is required.");

            try
            {
                string? imageUrl = null;
                string? publicId = null;

                Console.WriteLine($"ImageFile: {petDto.ImageFile?.FileName}, Length: {petDto.ImageFile?.Length}");
                if (petDto.ImageFile != null && petDto.ImageFile.Length > 0)
                {
                    (imageUrl, publicId) = await _uploadAssetsService.UploadImageAsync(petDto.ImageFile, "pets");
                }

                // Gọi service để cập nhật Pet
                var updatedPet = await _petService.UpdatePetAsync(id, petDto, imageUrl);
                if (updatedPet == null) return NotFound();
                return Ok(updatedPet);
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

        // PUT: api/pets/bulk : Cập nhật nhiều pet
        [Authorize(Roles = "Admin")]
        [HttpPut("bulk")]
        public async Task<IActionResult> UpdatePetsBulk([FromBody] List<UpdatePetDto> pets)
        {
            if (pets == null || !pets.Any()) return BadRequest("Danh sách pet rỗng.");
            var updatedPets = await _petService.UpdatePetsBulkAsync(pets);
            return Ok(updatedPets);
        }

        //------------------------------ DELETE -----------------------------------

        // DELETE: api/pets/{id} : Xóa pet theo ID
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePet(int id)
        {
            var result = await _petService.DeletePetAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        // DELETE: api/pets/bulk : Xóa nhiều pet
        [Authorize(Roles = "Admin")]
        [HttpDelete("bulk")]
        public async Task<IActionResult> DeletePetsBulk([FromBody] List<int> petIds)
        {
            if (petIds == null || !petIds.Any()) return BadRequest("Danh sách ID rỗng.");
            var result = await _petService.DeletePetsBulkAsync(petIds);
            return result ? NoContent() : BadRequest("Xóa thất bại.");
        }

    }
}
