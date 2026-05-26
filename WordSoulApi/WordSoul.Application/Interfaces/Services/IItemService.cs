using System.Threading;
using System.Threading.Tasks;
using WordSoul.Application.DTOs.Item;

namespace WordSoul.Application.Interfaces.Services
{
    /// <summary>
    /// Giao diện dịch vụ xử lý Item.
    /// </summary>
    public interface IItemService
    {
        Task<List<ItemDto>> GetAllItemsAsync(CancellationToken ct = default);
        Task<ItemDto?> GetItemByIdAsync(int id, CancellationToken ct = default);
        Task<ItemDto> CreateItemAsync(CreateItemDto createItemDto, string? imageUrl, CancellationToken ct = default);
        Task<ItemDto> UpdateItemAsync(int id, UpdateItemDto dto, string? imageUrl, CancellationToken ct = default);
        Task DeleteItemAsync(int id, CancellationToken ct = default);
    }
}

