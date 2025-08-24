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
        private readonly IUserVocabularySetService _userVocabularySetService;
        public UserController(IUserService userService, IUserVocabularySetService userVocabularySetService)
        {
            _userService = userService;
            _userVocabularySetService = userVocabularySetService;
        }

        //GET: api/User : Lấy tất cả người dùng
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        // GET: api/User/{id} : Lấy người dùng theo ID
        [Authorize(Roles = "Admin,User")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // PUT: api/User/{id} : Cập nhật người dùng theo ID
        [Authorize(Roles = "Admin,User")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserDto userDto)
        {
            if (userDto == null) return BadRequest("User data is required.");
            var updatedUser = await _userService.UpdateUserAsync(id, userDto);
            if (updatedUser == null) return NotFound();
            return Ok(updatedUser);
        }

        // DELETE: api/User/{id} : Xóa người dùng theo ID
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result) return NotFound();
            return NoContent();

        }

        

        // api lấy thông tin người dùng hiện tại

        // api lấy tiến trình học tập của người dùng

        //api lấy bộ từ vự của người dùng

        // api lấy danh sách pet user đã sở hữu

    }
}
