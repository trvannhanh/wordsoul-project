using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WordSoul.Application.DTOs.Pet;

namespace WordSoul.Application.Interfaces.Services
{
    /// <summary>
    /// Giao diện dịch vụ xử lý Pet.
    /// </summary>
    public interface IPetService
    {
        /// <summary>
        /// Tạo một Pet mới.
        /// </summary>
        Task<PetDto> CreatePetAsync(CreatePetDto petDto, string? imageUrl);

        /// <summary>
        /// Tạo nhiều Pet cùng lúc.
        /// </summary>
        Task<List<PetDto>> CreatePetsBulkAsync(BulkCreatePetDto bulkDto);

        /// <summary>
        /// Lấy chi tiết Pet của người dùng.
        /// </summary>
        Task<UserPetDetailDto?> GetPetDetailAsync(int userId, int petId);

        /// <summary>
        /// Lấy Pet theo ID.
        /// </summary>
        Task<PetDto?> GetPetByIdAsync(int id);

        /// <summary>
        /// Cập nhật thông tin Pet.
        /// </summary>
        Task<AdminPetDto> UpdatePetAsync(int id, UpdatePetDto dto, string? imageUrl);

        /// <summary>
        /// Cập nhật nhiều Pet cùng lúc.
        /// </summary>
        Task<List<PetDto>> UpdatePetsBulkAsync(List<UpdatePetDto> pets);

        /// <summary>
        /// Xoá một Pet theo ID.
        /// </summary>
        Task<bool> DeletePetAsync(int id);

        /// <summary>
        /// Xoá nhiều Pet theo danh sách ID.
        /// </summary>
        Task<bool> DeletePetsBulkAsync(List<int> petIds);
    }
}
