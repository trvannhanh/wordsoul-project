using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WordSoulApi.Extensions;
using WordSoulApi.Models.DTOs.Pet;
using WordSoulApi.Models.DTOs.User;
using WordSoulApi.Models.Entities;
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
        private readonly IUserOwnedPetService _userOwnedPetService;
        private readonly IUserVocabularyProgressService _userVocabularyProgressService;
        public UserController(IUserService userService, IUserVocabularySetService userVocabularySetService, 
                                IActivityLogService activityLogService, IPetService petService, 
                                IUserOwnedPetService userOwnedPetService, IUserVocabularyProgressService userVocabularyProgressService)
        {
            _userService = userService;
            _userVocabularySetService = userVocabularySetService;
            _activityLogService = activityLogService;
            _petService = petService;
            _userOwnedPetService = userOwnedPetService;
            _userVocabularyProgressService = userVocabularyProgressService;
        }

        //------------------------------ POST -----------------------------------


        // POST: api/users/{userId}/pets/{petId} : Gán pet cho user
        [Authorize(Roles = "Admin")]
        [HttpPost("{userId}/pets/{petId}")]
        public async Task<IActionResult> AssignPetToUser(int userId, int petId, [FromBody] AssignPetDto assignDto)
        {
            assignDto.UserId = userId;
            assignDto.PetId = petId;
            var result = await _userOwnedPetService.AssignPetToUserAsync(assignDto);
            if (result == null) return NotFound("User hoặc pet không tồn tại.");
            return Ok(result);
        }

        //------------------------------ GET -----------------------------------

        //GET: api/users : Lấy tất cả người dùng
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers(string? name, string?email, UserRole? role, int pageNumber = 1, int pageSize = 10)
        {
            var users = await _userService.GetAllUsersAsync(name, email, role, pageNumber, pageSize);
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

        //GET: api/users/progress: Lấy thông tin tổng hợp của người dùng
        [HttpGet("progress")]
        public async Task<IActionResult> GetUserProgress()
        {
            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();

            var progress = await _userVocabularyProgressService.GetUserProgressAsync(userId);
            return Ok(progress);
        }


        // GET: api/users/{userId}/activities : Lấy hoạt động của user
        [Authorize(Roles = "Admin")]
        [HttpGet("{userId}/activities")]
        public async Task<IActionResult> GetUserActivities(int userId, int pageNumber = 1, int pageSize = 10)
        {
            var activities = await _activityLogService.GetUserActivityLogsAsync(userId, pageNumber, pageSize);
            return Ok(activities);
        }

        // GET: api/activities : Lấy tất cả hoạt động hệ thống
        [Authorize(Roles = "Admin")]
        [HttpGet("activities")]
        public async Task<IActionResult> GetAllActivities(string? action = null, DateTime? fromDate = null, int pageNumber = 1, int pageSize = 10)
        {
            var activities = await _activityLogService.GetAllActivityLogsAsync(action, fromDate, pageNumber, pageSize);
            return Ok(activities);
        }

        // GET: api/users/leaderboard : Lấy bảng xếp hạng người dùng
        [HttpGet("leaderboard")]
        public async Task<IActionResult> GetLeaderBoard(bool? topXP, bool? topAP, int pageNumber = 1, int pageSize = 10)
        {
            var leaderboard = await _userService.GetLeaderBoardAsync(topXP, topAP, pageNumber, pageSize);
            return Ok(leaderboard);
        }

        //------------------------------ PUT -----------------------------------

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

        // PUT: api/users/{userId}/role : Gán role cho user
        [Authorize(Roles = "Admin")]
        [HttpPut("{userId}/role")]
        public async Task<IActionResult> AssignRoleToUser(int userId, [FromBody] AssignRoleDto assignDto)
        {
            var result = await _userService.AssignRoleToUserAsync(userId, assignDto.RoleName);
            if (!result) return NotFound("User not found or invalid role.");
            return Ok("Role assigned successfully.");
        }

        //------------------------------ DELETE -----------------------------------

        // DELETE: api/users/{id} : Xóa người dùng theo ID
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result) return NotFound();
            return NoContent();

        }

        // DELETE: api/users/{userId}/pets/{petId} : Xóa gán pet
        [Authorize(Roles = "Admin")]
        [HttpDelete("{userId}/pets/{petId}")]
        public async Task<IActionResult> RemovePetFromUser(int userId, int petId)
        {
            var result = await _userOwnedPetService.RemovePetFromUserAsync(userId, petId);
            return result ? NoContent() : NotFound("Gán pet không tồn tại.");
        }

        [Authorize(Roles = "Admin,User")]
        [HttpGet("vocabulary-sets/{vocabularySetId}")]
        public async Task<IActionResult> GetUserVocabularySetProgress(int vocabularySetId)
        {
            try
            {
                var userId = User.GetUserId();
                if (userId == 0) return Unauthorized();
                var progress = await _userVocabularySetService.GetUserVocabularySetAsync(userId, vocabularySetId);
                if (progress == null) return NotFound("No progress found for the specified vocabulary set.");
                return Ok(progress);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while getting the user vocabulary set.", Error = ex.Message });
            }

        }


    }
}
