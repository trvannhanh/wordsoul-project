using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using WordSoul.Api.Extensions;
using WordSoul.Application.DTOs.UserGroup;
using WordSoul.Application.Interfaces.Services;

namespace WordSoul.Api.Controllers
{
    [Route("api/admin/groups")]
    [ApiController]
    [EnableCors("AllowFrontend")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminGroupController : ControllerBase
    {
        private readonly IUserGroupService _groupService;

        public AdminGroupController(IUserGroupService groupService)
        {
            _groupService = groupService;
        }

        // GET: api/admin/groups
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 15,
            [FromQuery] string? search = null,
            CancellationToken ct = default)
        {
            var groups = await _groupService.GetAllAsync(pageNumber, pageSize, search, ct);
            return Ok(groups);
        }

        // GET: api/admin/groups/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
        {
            try
            {
                var group = await _groupService.GetByIdAsync(id, ct);
                return Ok(group);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // POST: api/admin/groups
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserGroupDto dto, CancellationToken ct = default)
        {
            var createdByUserId = User.GetUserId();
            if (createdByUserId == 0) return Unauthorized();

            var group = await _groupService.CreateAsync(createdByUserId, dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = group.Id }, group);
        }

        // PUT: api/admin/groups/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserGroupDto dto, CancellationToken ct = default)
        {
            try
            {
                var group = await _groupService.UpdateAsync(id, dto, ct);
                return Ok(group);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // DELETE: api/admin/groups/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
        {
            var result = await _groupService.DeleteAsync(id, ct);
            if (!result) return NotFound();
            return NoContent();
        }

        // POST: api/admin/groups/{id}/members
        [HttpPost("{id}/members")]
        public async Task<IActionResult> AddMember(int id, [FromBody] AddGroupMemberDto dto, CancellationToken ct = default)
        {
            var result = await _groupService.AddMemberAsync(id, dto.UserId, ct);
            if (!result) return BadRequest("User is already a member or group not found.");
            return Ok(new { Message = "Member added successfully." });
        }

        // DELETE: api/admin/groups/{id}/members/{userId}
        [HttpDelete("{id}/members/{userId}")]
        public async Task<IActionResult> RemoveMember(int id, int userId, CancellationToken ct = default)
        {
            var result = await _groupService.RemoveMemberAsync(id, userId, ct);
            if (!result) return NotFound("Member not found in this group.");
            return NoContent();
        }
    }
}
