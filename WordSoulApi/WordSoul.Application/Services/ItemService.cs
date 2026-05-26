using Microsoft.Extensions.Logging;
using WordSoul.Application.DTOs.Item;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;

namespace WordSoul.Application.Services
{
    /// <summary>
    /// Cung cấp các chức năng xử lý Item như tạo mới, cập nhật, xoá và truy vấn dữ liệu Item.
    /// </summary>
    public class ItemService : IItemService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<ItemService> _logger;

        /// <summary>
        /// Khởi tạo <see cref="ItemService"/> với UnitOfWork và Logger.
        /// </summary>
        /// <param name="uow">Đối tượng UnitOfWork cho phép thao tác repository và commit transaction.</param>
        /// <param name="logger">Logger phục vụ ghi log.</param>
        public ItemService(IUnitOfWork uow, ILogger<ItemService> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        // ---------------------------------------------------------------------
        // READ
        // ---------------------------------------------------------------------

        public async Task<List<ItemDto>> GetAllItemsAsync(CancellationToken ct = default)
        {
            var items = await _uow.Item.GetItemAsync(ct);
            return items.Select(i => new ItemDto
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                ImageUrl = i.ImageUrl,
                Type = i.Type.ToString()
            }).ToList();
        }

        public async Task<ItemDto?> GetItemByIdAsync(int id, CancellationToken ct = default)
        {
            var item = await _uow.Item.GetItemByIdAsync(id, ct);
            if (item == null) return null;
            return new ItemDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                ImageUrl = item.ImageUrl,
                Type = item.Type.ToString()
            };
        }

        // ---------------------------------------------------------------------
        // CREATE
        // ---------------------------------------------------------------------

        /// <summary>
        /// Tạo mới một Item và lưu vào cơ sở dữ liệu thông qua UnitOfWork.
        /// </summary>
        /// <param name="createItemDto">Thông tin dùng để tạo Item.</param>
        /// <param name="imageUrl">Đường dẫn ảnh của Item, có thể null.</param>
        /// <param name="ct">Token hỗ trợ huỷ thao tác bất đồng bộ.</param>
        /// <returns>
        /// Trả về <see cref="ItemDto"/> chứa thông tin Item vừa tạo.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Ném ra khi Name không hợp lệ hoặc thiếu.
        /// </exception>
        /// <exception cref="Exception">
        /// Ném ra khi xảy ra lỗi trong quá trình tạo Item.
        /// </exception>
        public async Task<ItemDto> CreateItemAsync(
            CreateItemDto createItemDto,
            string? imageUrl,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(createItemDto.Name))
                throw new ArgumentException("Name is required.", nameof(createItemDto.Name));

            try
            {
                _logger.LogInformation("Creating item: {Name}", createItemDto.Name);

                var item = new Item
                {
                    Name = createItemDto.Name,
                    Description = createItemDto.Description,
                    ImageUrl = imageUrl,
                    Type = createItemDto.Type
                };

                await _uow.Item.CreateItemAsync(item, ct);

                await _uow.SaveChangesAsync(ct);

                _logger.LogInformation("Successfully created item {Name}", item.Name);

                return new ItemDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    ImageUrl = item.ImageUrl,
                    Type = item.Type.ToString()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating item: {Name}", createItemDto.Name);
                throw new Exception($"Error creating item: {ex.Message}", ex);
            }
        }

        // ---------------------------------------------------------------------
        // UPDATE
        // ---------------------------------------------------------------------

        public async Task<ItemDto> UpdateItemAsync(int id, UpdateItemDto dto, string? imageUrl, CancellationToken ct = default)
        {
            var item = await _uow.Item.GetItemByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"Item {id} not found.");

            item.Name = dto.Name;
            item.Description = dto.Description;
            item.Type = dto.Type;
            if (imageUrl != null)
                item.ImageUrl = imageUrl;

            await _uow.Item.UpdateItemAsync(item, ct);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("Updated item {Id} — {Name}", item.Id, item.Name);

            return new ItemDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                ImageUrl = item.ImageUrl,
                Type = item.Type.ToString()
            };
        }

        // ---------------------------------------------------------------------
        // DELETE
        // ---------------------------------------------------------------------

        public async Task DeleteItemAsync(int id, CancellationToken ct = default)
        {
            var item = await _uow.Item.GetItemByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"Item {id} not found.");

            await _uow.Item.DeleteItemAsync(id, ct);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("Deleted item {Id} — {Name}", id, item.Name);
        }
    }
}
