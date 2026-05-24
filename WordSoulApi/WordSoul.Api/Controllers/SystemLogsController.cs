using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WordSoul.Infrastructure.Persistence;
using WordSoul.Domain.Entities;

namespace WordSoul.Api.Controllers
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin, SuperAdmin")]
    public class SystemLogsController : ControllerBase
    {
        private readonly WordSoulDbContext _context;

        public SystemLogsController(WordSoulDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetSystemLogs(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? method = null,
            [FromQuery] int? statusCode = null)
        {
            var query = _context.SystemLogs.AsQueryable();

            if (!string.IsNullOrEmpty(method))
            {
                query = query.Where(l => l.Method == method);
            }

            if (statusCode.HasValue)
            {
                query = query.Where(l => l.StatusCode == statusCode.Value);
            }

            var totalItems = await query.CountAsync();

            var logs = await query
                .OrderByDescending(l => l.Timestamp)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new
                {
                    l.Id,
                    l.Timestamp,
                    l.Method,
                    l.Path,
                    l.StatusCode,
                    l.DurationMs,
                    l.IpAddress,
                    l.UserId
                })
                .ToListAsync();

            return Ok(new
            {
                Items = logs,
                TotalItems = totalItems,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSystemLogDetail(int id)
        {
            var log = await _context.SystemLogs.FindAsync(id);

            if (log == null)
            {
                return NotFound();
            }

            return Ok(log);
        }
    }
}
