using Microsoft.Extensions.Logging;
using WordSoul.Application.DTOs.Item;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;

namespace WordSoul.Application.Services
{
    public class ItemService : IItemService
    {

        private readonly IItemRepository _itemRepository;
        private readonly ILogger<ItemService> _logger;

        public ItemService(ILogger<ItemService> logger, IItemRepository itemRepository)
        {
            _itemRepository = itemRepository;
            _logger = logger;
        }

        //---------------------CREATE-------------------
        public async Task<ItemDto> CreateItemAsync(CreateItemDto createItemDto, string? imageUrl)
        {
            _logger.LogInformation("Creating item with Name: {Name}", createItemDto.Name);

            if (string.IsNullOrWhiteSpace(createItemDto.Name))
            {
                _logger.LogError("Item Name is required for creating Item");
                throw new ArgumentException("Name is required.", nameof(createItemDto.Name));
            }

            try
            {
                _logger.LogInformation("Creating Item {ItemName}", createItemDto.Name);

                var item = new Item
                {
                    Name = createItemDto.Name,
                    Description = createItemDto.Description,
                    ImageUrl = imageUrl,
                    Type = createItemDto.Type

                };

                await _itemRepository.CreateItemAsync(item);
                _logger.LogInformation("Successfully created item {ItemName}", item.Name);

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
                _logger.LogError(ex, "Error creating item");
                throw new Exception($"Error creating item : {ex.Message}", ex);
            }
        }
    }
}
