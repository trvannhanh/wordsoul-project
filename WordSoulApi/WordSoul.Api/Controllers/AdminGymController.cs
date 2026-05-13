using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Application.DTOs.Gym;
using WordSoul.Application.Interfaces;
using WordSoul.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace WordSoul.Api.Controllers
{
    [Route("api/admin/gyms")]
    [ApiController]
    [EnableCors("AllowFrontend")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminGymController : ControllerBase
    {
        private readonly IGymLeaderService _gymService;

        public AdminGymController(IGymLeaderService gymService)
        {
            _gymService = gymService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllGyms()
        {
            var gyms = await _gymService.AdminGetAllGymsAsync();
            return Ok(gyms);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGym(int id, [FromBody] GymUpdateDto dto)
        {
            try
            {
                var gym = await _gymService.AdminUpdateGymAsync(id, dto);
                return Ok(gym);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
