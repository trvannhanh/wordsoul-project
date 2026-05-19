using Microsoft.Extensions.Logging;
using WordSoul.Application.DTOs.UserGroup;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;

namespace WordSoul.Application.Services
{
    public class UserGroupService : IUserGroupService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<UserGroupService> _logger;

        public UserGroupService(IUnitOfWork uow, ILogger<UserGroupService> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task<IEnumerable<UserGroupDto>> GetAllAsync(
            int pageNumber, int pageSize, string? search,
            CancellationToken ct = default)
        {
            var groups = await _uow.UserGroup.GetAllAsync(pageNumber, pageSize, search, ct);
            return groups.Select(g => new UserGroupDto
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                MemberCount = g.Members.Count,
                CreatedByUsername = g.CreatedByUser?.Username ?? string.Empty,
                CreatedAt = g.CreatedAt,
            });
        }

        public async Task<UserGroupDetailDto> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var group = await _uow.UserGroup.GetByIdWithMembersAsync(id, ct)
                ?? throw new KeyNotFoundException($"Group with ID {id} not found.");

            return new UserGroupDetailDto
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                MemberCount = group.Members.Count,
                CreatedByUsername = group.CreatedByUser?.Username ?? string.Empty,
                CreatedAt = group.CreatedAt,
                Members = group.Members.Select(m => new GroupMemberDto
                {
                    UserId = m.UserId,
                    Username = m.User?.Username ?? string.Empty,
                    Email = m.User?.Email ?? string.Empty,
                    Role = m.User?.Role.ToString() ?? string.Empty,
                    JoinedAt = m.JoinedAt,
                }).ToList(),
            };
        }

        public async Task<UserGroupDto> CreateAsync(
            int createdByUserId, CreateUserGroupDto dto,
            CancellationToken ct = default)
        {
            var group = new UserGroup
            {
                Name = dto.Name,
                Description = dto.Description,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow,
            };

            await _uow.UserGroup.CreateAsync(group, ct);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("UserGroup '{Name}' created by user {UserId}", group.Name, createdByUserId);

            return new UserGroupDto
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                MemberCount = 0,
                CreatedByUsername = string.Empty,
                CreatedAt = group.CreatedAt,
            };
        }

        public async Task<UserGroupDto> UpdateAsync(
            int id, UpdateUserGroupDto dto,
            CancellationToken ct = default)
        {
            var group = await _uow.UserGroup.GetByIdWithMembersAsync(id, ct)
                ?? throw new KeyNotFoundException($"Group with ID {id} not found.");

            group.Name = dto.Name;
            group.Description = dto.Description;

            await _uow.UserGroup.UpdateAsync(group, ct);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("UserGroup {GroupId} updated", id);

            return new UserGroupDto
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                MemberCount = group.Members.Count,
                CreatedByUsername = group.CreatedByUser?.Username ?? string.Empty,
                CreatedAt = group.CreatedAt,
            };
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var group = await _uow.UserGroup.GetByIdWithMembersAsync(id, ct);
            if (group == null) return false;

            await _uow.UserGroup.DeleteAsync(group, ct);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("UserGroup {GroupId} deleted", id);
            return true;
        }

        public async Task<bool> AddMemberAsync(int groupId, int userId, CancellationToken ct = default)
        {
            var group = await _uow.UserGroup.GetByIdWithMembersAsync(groupId, ct);
            if (group == null) return false;

            var alreadyMember = await _uow.UserGroup.MemberExistsAsync(groupId, userId, ct);
            if (alreadyMember) return false;

            var member = new UserGroupMember
            {
                UserGroupId = groupId,
                UserId = userId,
                JoinedAt = DateTime.UtcNow,
            };

            await _uow.UserGroup.AddMemberAsync(member, ct);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("User {UserId} added to group {GroupId}", userId, groupId);
            return true;
        }

        public async Task<bool> RemoveMemberAsync(int groupId, int userId, CancellationToken ct = default)
        {
            var exists = await _uow.UserGroup.MemberExistsAsync(groupId, userId, ct);
            if (!exists) return false;

            await _uow.UserGroup.RemoveMemberAsync(groupId, userId, ct);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("User {UserId} removed from group {GroupId}", userId, groupId);
            return true;
        }
    }
}
