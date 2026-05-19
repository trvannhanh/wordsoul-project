using Microsoft.EntityFrameworkCore;
using WordSoul.Application.Interfaces.Repositories;
using WordSoul.Domain.Entities;

namespace WordSoul.Infrastructure.Persistence.Repositories
{
    public class UserGroupRepository : IUserGroupRepository
    {
        private readonly WordSoulDbContext _context;

        public UserGroupRepository(WordSoulDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserGroup>> GetAllAsync(
            int pageNumber, int pageSize, string? search,
            CancellationToken ct = default)
        {
            var query = _context.UserGroups
                .Include(g => g.CreatedByUser)
                .Include(g => g.Members)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(g => g.Name.Contains(search) ||
                                         (g.Description != null && g.Description.Contains(search)));

            return await query
                .OrderByDescending(g => g.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);
        }

        public async Task<UserGroup?> GetByIdWithMembersAsync(int id, CancellationToken ct = default)
        {
            return await _context.UserGroups
                .Include(g => g.CreatedByUser)
                .Include(g => g.Members)
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(g => g.Id == id, ct);
        }

        public async Task CreateAsync(UserGroup group, CancellationToken ct = default)
        {
            await _context.UserGroups.AddAsync(group, ct);
        }

        public Task UpdateAsync(UserGroup group, CancellationToken ct = default)
        {
            _context.UserGroups.Update(group);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(UserGroup group, CancellationToken ct = default)
        {
            _context.UserGroups.Remove(group);
            return Task.CompletedTask;
        }

        public async Task<bool> MemberExistsAsync(int groupId, int userId, CancellationToken ct = default)
        {
            return await _context.UserGroupMembers
                .AnyAsync(m => m.UserGroupId == groupId && m.UserId == userId, ct);
        }

        public async Task AddMemberAsync(UserGroupMember member, CancellationToken ct = default)
        {
            await _context.UserGroupMembers.AddAsync(member, ct);
        }

        public async Task RemoveMemberAsync(int groupId, int userId, CancellationToken ct = default)
        {
            var member = await _context.UserGroupMembers
                .FirstOrDefaultAsync(m => m.UserGroupId == groupId && m.UserId == userId, ct);
            if (member != null)
                _context.UserGroupMembers.Remove(member);
        }
    }
}
