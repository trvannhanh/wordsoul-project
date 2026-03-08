
using WordSoul.Application.DTOs.Pet;

namespace WordSoul.Application.Interfaces.Services
{
    public interface IPetBuffService
    {
        /// <summary>
        /// Tính và trả về buff của active pet hiện tại của user.
        /// Trả về null nếu user chưa có active pet.
        /// </summary>
        Task<PetBuffDto?> GetActivePetBuffAsync(int userId, CancellationToken ct = default);
    }
}
