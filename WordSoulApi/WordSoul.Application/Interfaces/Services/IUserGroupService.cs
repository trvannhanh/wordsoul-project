using WordSoul.Application.DTOs.UserGroup;

namespace WordSoul.Application.Interfaces.Services
{
    public interface IUserGroupService
    {
        Task<IEnumerable<UserGroupDto>> GetAllAsync(int pageNumber, int pageSize, string? search, CancellationToken ct = default);
        Task<UserGroupDetailDto> GetByIdAsync(int id, CancellationToken ct = default);
        Task<UserGroupDto> CreateAsync(int createdByUserId, CreateUserGroupDto dto, CancellationToken ct = default);
        Task<UserGroupDto> UpdateAsync(int id, UpdateUserGroupDto dto, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
        Task<bool> AddMemberAsync(int groupId, int userId, CancellationToken ct = default);
        Task<bool> RemoveMemberAsync(int groupId, int userId, CancellationToken ct = default);
    }
}
