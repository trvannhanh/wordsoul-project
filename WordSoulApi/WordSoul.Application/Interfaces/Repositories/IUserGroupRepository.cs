using WordSoul.Domain.Entities;

namespace WordSoul.Application.Interfaces.Repositories
{
    public interface IUserGroupRepository
    {
        Task<IEnumerable<UserGroup>> GetAllAsync(int pageNumber, int pageSize, string? search, CancellationToken ct = default);
        Task<UserGroup?> GetByIdWithMembersAsync(int id, CancellationToken ct = default);
        Task CreateAsync(UserGroup group, CancellationToken ct = default);
        Task UpdateAsync(UserGroup group, CancellationToken ct = default);
        Task DeleteAsync(UserGroup group, CancellationToken ct = default);
        Task<bool> MemberExistsAsync(int groupId, int userId, CancellationToken ct = default);
        Task AddMemberAsync(UserGroupMember member, CancellationToken ct = default);
        Task RemoveMemberAsync(int groupId, int userId, CancellationToken ct = default);
    }
}
