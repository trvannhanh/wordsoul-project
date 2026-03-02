using System.Threading;
using System.Threading.Tasks;
using WordSoul.Domain.Entities;

namespace WordSoul.Application.Interfaces.Services
{
    /// <summary>
    /// Giao diện dịch vụ xử lý logic Pet phần thưởng dựa trên VocabularySet.
    /// </summary>
    public interface ISetRewardPetService
    {
        /// <summary>
        /// Lấy ngẫu nhiên một Pet làm phần thưởng dựa trên VocabularySetId
        /// và milestone học tập hiện tại của người dùng.
        /// Pet hiếm (DropRate thấp) sẽ có xác suất tăng dần theo milestone.
        /// </summary>
        /// <param name="vocabularySetId">ID của bộ từ vựng cần lấy phần thưởng.</param>
        /// <param name="milestone">Số lần người dùng đã hoàn thành bộ từ vựng.</param>
        /// <param name="cancellationToken">Token hủy thao tác bất đồng bộ.</param>
        /// <returns>
        /// Trả về Pet được chọn làm phần thưởng,
        /// hoặc <c>null</c> nếu bộ từ vựng không cấu hình pet thưởng.
        /// </returns>
        Task<Pet?> GetRandomPetBySetIdAsync(
            int vocabularySetId,
            int milestone,
            CancellationToken cancellationToken = default);
    }
}
