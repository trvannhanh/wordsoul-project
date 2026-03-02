using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WordSoul.Application.DTOs.Pet;
using WordSoul.Domain.Entities;
using WordSoul.Domain.Enums;

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
        /// Lấy danh sách Pet, kèm trạng thái người dùng đã sở hữu hay chưa.
        /// Hỗ trợ filter theo tên, độ hiếm, loại, sở hữu, vocabularySetId và phân trang.
        /// </summary>
        Task<IEnumerable<UserPetDto>> GetAllPetsAsync(
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
