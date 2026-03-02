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
        /// <summary>
        /// Tạo mới một Item.
        /// </summary>
        Task<ItemDto> CreateItemAsync(
            CreateItemDto createItemDto,
            string? imageUrl,
            CancellationToken ct = default);
    }
}
