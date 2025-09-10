using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WordSoulApi.Extensions;
using WordSoulApi.Models.DTOs.Pet;
using WordSoulApi.Models.DTOs.User;
using WordSoulApi.Services.Implementations;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Controllers
{
    [Route("api/users")]
    [ApiController]
    [EnableCors("AllowLocalhost")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserVocabularySetService _userVocabularySetService;
        private readonly IActivityLogService _activityLogService;
        private readonly IPetService _petService;
        public UserController(IUserService userService, IUserVocabularySetService userVocabularySetService, IActivityLogService activityLogService, IPetService petService)
        {
            _userService = userService;
            _userVocabularySetService = userVocabularySetService;
            _activityLogService = activityLogService;
            _petService = petService;
        }

        //GET: api/users : Lấy tất cả người dùng
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        // GET: api/users/{id} : Lấy người dùng theo ID
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // GET: api/users/me : Lấy thông tin người dùng hiện tại
        [Authorize(Roles = "Admin,User")]
        [HttpGet("me")]
        public async Task<IActionResult> GetUserById()
        {
            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // PUT: api/users/{id} : Cập nhật người dùng theo ID
        [Authorize(Roles = "Admin,User")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserDto userDto)
        {
            if (userDto == null) return BadRequest("User data is required.");
            var updatedUser = await _userService.UpdateUserAsync(id, userDto);
            if (updatedUser == null) return NotFound();
            return Ok(updatedUser);
        }

        // DELETE: api/users/{id} : Xóa người dùng theo ID
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result) return NotFound();
            return NoContent();

        }

        //GET: api/users/user-dashboard: Lấy thông tin tổng hợp của người dùng
        [HttpGet("user-dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();

            var dashboard = await _userService.GetUserDashboardAsync(userId);
            return Ok(dashboard);
        }

        // PUT: api/users/{userId}/role : Gán role cho user
        [Authorize(Roles = "Admin")]
        [HttpPut("{userId}/role")]
        public async Task<IActionResult> AssignRoleToUser(int userId, [FromBody] AssignRoleDto assignDto)
        {
            var result = await _userService.AssignRoleToUserAsync(userId, assignDto.RoleName);
            if (!result) return NotFound("User not found or invalid role.");
            return Ok("Role assigned successfully.");
        }


        // GET: api/users/{userId}/activities : Lấy hoạt động của user
        [Authorize(Roles = "Admin")]
        [HttpGet("{userId}/activities")]
        public async Task<IActionResult> GetUserActivities(int userId, int pageNumber = 1, int pageSize = 10)
        {
            var activities = await _activityLogService.GetUserActivitiesAsync(userId, pageNumber, pageSize);
            return Ok(activities);
        }

        // GET: api/activities : Lấy tất cả hoạt động hệ thống
        [Authorize(Roles = "Admin")]
        [HttpGet("activities")]
        public async Task<IActionResult> GetAllActivities(string? action = null, DateTime? fromDate = null, int pageNumber = 1, int pageSize = 10)
        {
            var activities = await _activityLogService.GetAllActivitiesAsync(action, fromDate, pageNumber, pageSize);
            return Ok(activities);
        }

        // POST: api/users/{userId}/pets/{petId} : Gán pet cho user
        [Authorize(Roles = "Admin")]
        [HttpPost("{userId}/pets/{petId}")]
        public async Task<IActionResult> AssignPetToUser(int userId, int petId, [FromBody] AssignPetDto assignDto)
        {
            assignDto.UserId = userId;
            assignDto.PetId = petId;
            var result = await _petService.AssignPetToUserAsync(assignDto);
            if (result == null) return NotFound("User hoặc pet không tồn tại.");
            return Ok(result);
        }

        // DELETE: api/users/{userId}/pets/{petId} : Xóa gán pet
        [Authorize(Roles = "Admin")]
        [HttpDelete("{userId}/pets/{petId}")]
        public async Task<IActionResult> RemovePetFromUser(int userId, int petId)
        {
            var result = await _petService.RemovePetFromUserAsync(userId, petId);
            return result ? NoContent() : NotFound("Gán pet không tồn tại.");
        }

    }
}
