using WordSoulApi.Models.Entities;

namespace WordSoulApi.Repositories.Interfaces
{
    public interface IUserOwnedPetRepository
    {
        // Thêm pet vào danh sách sở hữu của người dùng
        Task AddPetToUserAsync(UserOwnedPet userOwnedPet);
        // kiểm tra người dùng có sở hữu pet này chưa
        Task<bool> CheckPetOwnedByUserAsync(int userId, int petId);
    }
}