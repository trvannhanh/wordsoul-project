using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using WordSoulApi.Models.DTOs.ItemDto;
using WordSoulApi.Models.DTOs.Pet;
using WordSoulApi.Services.Implementations;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Controllers
{
    [Route("api/items")]
    [ApiController]
    [EnableCors("AllowLocalhost")]
    public class ItemController : ControllerBase
    {
        private readonly IItemService _itemService;
        private readonly IUploadAssetsService _uploadAssetsService;

        public ItemController(IItemService itemService, IUploadAssetsService uploadAssetsService)
        {
            _itemService = itemService;
            _uploadAssetsService = uploadAssetsService;
        }

        // POST: api/items : Tạo item mới
        //[Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateItem([FromForm] CreateItemDto createItemDto)
        {

            if (createItemDto == null)
            {
                return BadRequest("Item data is required.");
            }

            try
            {
                string? imageUrl = null;
                string? publicId = null;

                Console.WriteLine($"ImageFile: {createItemDto.ImageFile?.FileName}, Length: {createItemDto.ImageFile?.Length}");
                if (createItemDto.ImageFile != null && createItemDto.ImageFile.Length > 0)
                {
                    (imageUrl, publicId) = await _uploadAssetsService.UploadImageAsync(createItemDto.ImageFile, "items");
                }

                // Gọi service để tạo Item
                var createdItem = await _itemService.CreateItemAsync(createItemDto, imageUrl);

                return Ok(createdItem);
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
                return StatusCode(500, new { Message = "An error occurred while creating the item.", Error = ex.Message });
            }
        }
    }
}
