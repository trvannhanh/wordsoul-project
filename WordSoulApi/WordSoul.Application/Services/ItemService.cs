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
    }
}
