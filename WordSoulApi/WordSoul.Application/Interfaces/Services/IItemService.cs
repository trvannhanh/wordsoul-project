using WordSoul.Application.DTOs.Item;

namespace WordSoul.Application.Interfaces.Services
{
    public interface IItemService
    {
        Task<ItemDto> CreateItemAsync(CreateItemDto createItemDto, string? imageUrl);
    }
}