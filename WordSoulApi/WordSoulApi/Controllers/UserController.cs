using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WordSoulApi.Extensions;
using WordSoulApi.Models.DTOs.User;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // chỉ dành cho admin
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        //Authorized
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        //Authorized
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserDto userDto)
        {
            if (userDto == null) return BadRequest("User data is required.");
            var updatedUser = await _userService.UpdateUserAsync(id, userDto);
            if (updatedUser == null) return NotFound();
            return Ok(updatedUser);
        }

        // chỉ dành cho admin
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result) return NotFound();
            return NoContent();

        }

        [Authorize(Roles = "User")]
        [HttpPost("me/vocabulary-sets/{vocabId}")]
        public async Task<IActionResult> AddVocabularySet(int vocabId)
        {
            var userId = User.GetUserId();
            if (userId == 0)
                return Unauthorized();
            await _userService.AddVocabularySetToUserAsync(userId, vocabId);
            return Ok(new { message = "VocabularySet added successfully" });
        }

        // api lấy thông tin người dùng hiện tại

        // api lấy tiến trình học tập của người dùng

        //api lấy bộ từ vự của người dùng

        // api lấy danh sách pet user đã sở hữu

    }
}
