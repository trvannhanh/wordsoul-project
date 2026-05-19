using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using WordSoul.Application.DTOs.Item;
using WordSoul.Application.Interfaces.Services;

namespace WordSoul.Api.Controllers
{
    [Route("api/items")]
    [ApiController]
    [EnableCors("AllowFrontend")]
    public class ItemController : ControllerBase
    {
        private readonly IItemService _itemService;
        private readonly IUploadAssetsService _uploadAssetsService;

        public ItemController(IItemService itemService, IUploadAssetsService uploadAssetsService)
        {
            _itemService = itemService;
            _uploadAssetsService = uploadAssetsService;
        }

        // GET: api/items
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetAllItems(CancellationToken ct = default)
        {
            var items = await _itemService.GetAllItemsAsync(ct);
            return Ok(items);
        }

        // GET: api/items/{id}
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetItem(int id, CancellationToken ct = default)
        {
            var item = await _itemService.GetItemByIdAsync(id, ct);
            if (item == null) return NotFound();
            return Ok(item);
        }

        // POST: api/items
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> CreateItem([FromForm] CreateItemDto createItemDto, CancellationToken ct = default)
        {
            if (createItemDto == null) return BadRequest("Item data is required.");

            try
            {
                string? imageUrl = null;
                if (createItemDto.ImageFile != null && createItemDto.ImageFile.Length > 0)
                    (imageUrl, _) = await _uploadAssetsService.UploadImageAsync(createItemDto.ImageFile, "items");

                var createdItem = await _itemService.CreateItemAsync(createItemDto, imageUrl, ct);
                return Ok(createdItem);
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (Exception ex) { return StatusCode(500, new { Message = "Error creating item.", Error = ex.Message }); }
        }

        // PUT: api/items/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> UpdateItem(int id, [FromForm] UpdateItemDto dto, CancellationToken ct = default)
        {
            try
            {
                string? imageUrl = null;
                if (dto.ImageFile != null && dto.ImageFile.Length > 0)
                    (imageUrl, _) = await _uploadAssetsService.UploadImageAsync(dto.ImageFile, "items");

                var updated = await _itemService.UpdateItemAsync(id, dto, imageUrl, ct);
                return Ok(updated);
            }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (Exception ex) { return StatusCode(500, new { Message = "Error updating item.", Error = ex.Message }); }
        }

        // DELETE: api/items/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> DeleteItem(int id, CancellationToken ct = default)
        {
            try
            {
                await _itemService.DeleteItemAsync(id, ct);
                return NoContent();
            }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (Exception ex) { return StatusCode(500, new { Message = "Error deleting item.", Error = ex.Message }); }
        }
    }
}

