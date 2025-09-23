using WordSoulApi.Models.DTOs.ItemDto;

namespace WordSoulApi.Services.Interfaces
{
    public interface IItemService
    {
        Task<ItemDto> CreateItemAsync(CreateItemDto createItemDto, string? imageUrl);
    }
}