using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface IPetRepository
    {
        // ----------------------------- CREATE -----------------------------
        /// <summary>
        /// Tạo mới một Pet.
        /// </summary>
        Task<Pet> CreatePetAsync(
            Pet pet,
            CancellationToken cancellationToken = default);

        // ----------------------------- READ --------------------------------
        /// <summary>
        /// Lấy danh sách Pet, kèm trạng thái người dùng đã sở hữu hay chưa.
        /// Hỗ trợ filter theo tên, độ hiếm, loại, sở hữu, vocabularySetId và phân trang.
        /// </summary>
        Task<IEnumerable<(Pet Pet, bool IsOwned)>> GetAllPetsAsync(
            int userId,
            string? name = null,
            PetRarity? rarity = null,
            PetType? type = null,
            bool? isOwned = null,
            int? vocabularySetId = null,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy Pet theo ID.
        /// </summary>
        Task<Pet?> GetPetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách Pet ngẫu nhiên theo độ hiếm (rarity),
        /// ưu tiên các PetType phù hợp với Theme của bộ từ vựng.
        /// Nếu không đủ pet theo type ưu tiên, tự động fallback về rarity bất kỳ type.
        /// </summary>
        Task<List<Pet>> GetRandomPetsByRarityAsync(
            PetRarity rarity,
            int count,
            IEnumerable<PetType>? preferredTypes = null,
            CancellationToken cancellationToken = default);

        // ----------------------------- UPDATE ------------------------------
        /// <summary>
        /// Cập nhật thông tin Pet.
        /// </summary>
        Task<Pet> UpdatePetAsync(
            Pet pet,
            CancellationToken cancellationToken = default);

        // ----------------------------- DELETE ------------------------------
        /// <summary>
        /// Xóa Pet theo ID. Trả về true nếu xóa thành công.
        /// </summary>
        Task<bool> DeletePetAsync(
            int id,
            CancellationToken cancellationToken = default);
    }
}