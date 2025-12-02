using WordSoul.Domain.Entities;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface IUserOwnedPetRepository
    {
        // ----------------------------- CREATE -----------------------------
        /// <summary>
        /// Thêm một pet mới vào danh sách sở hữu của người dùng.
        /// </summary>
        Task CreateUserOwnedPetAsync(
            UserOwnedPet userOwnedPet,
            CancellationToken cancellationToken = default);

        // ----------------------------- READ -------------------------------
        /// <summary>
        /// Lấy thông tin sở hữu pet theo UserId và PetId (kèm thông tin chi tiết Pet).
        /// </summary>
        Task<UserOwnedPet?> GetUserOwnedPetByUserAndPetIdAsync(
            int userId,
            int petId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy toàn bộ danh sách pet mà người dùng đang sở hữu.
        /// </summary>
        Task<IEnumerable<UserOwnedPet>> GetAllUserOwnedPetByUserIdAsync(
            int userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy pet đang được kích hoạt (IsActive = true) của người dùng.
        /// </summary>
        Task<Pet?> GetActivePetByUserIdAsync(
            int userId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy ngẫu nhiên một pet từ bộ thưởng (SetRewardPet) theo VocabularySetId dựa trên DropRate.
        /// </summary>
        Task<Pet?> GetRandomPetBySetIdAsync(
            int vocabularySetId,
            CancellationToken cancellationToken = default);

        // ----------------------------- UPDATE -----------------------------
        /// <summary>
        /// Cập nhật thông tin sở hữu pet (ví dụ: đổi pet active, cập nhật level, exp…).
        /// </summary>
        Task UpdateUserOwnedPetAsync(
            UserOwnedPet userOwnedPet,
            CancellationToken cancellationToken = default);

        // ----------------------------- DELETE -----------------------------
        /// <summary>
        /// Xóa bỏ quyền sở hữu một pet của người dùng.
        /// </summary>
        Task DeleteUserOwnedPetAsync(
            UserOwnedPet userOwnedPet,
            CancellationToken cancellationToken = default);

        // ----------------------------- OTHER -----------------------------
        /// <summary>
        /// Kiểm tra xem người dùng đã sở hữu pet này chưa.
        /// </summary>
        Task<bool> CheckPetOwnedByUserAsync(
            int userId,
            int petId,
            CancellationToken cancellationToken = default);
    }
}