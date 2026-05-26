using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WordSoul.Infrastructure.Persistence;

namespace WordSoul.Api.Middlewares
{
    public class ActivityTrackingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ActivityTrackingMiddleware> _logger;

        public ActivityTrackingMiddleware(RequestDelegate next, ILogger<ActivityTrackingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Proceed with the request
            await _next(context);

            // After the request, update LastActiveAt if user is authenticated
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    try
                    {
                        using var scope = context.RequestServices.CreateScope();
                        var dbContext = scope.ServiceProvider.GetRequiredService<WordSoulDbContext>();
                        
                        // Cập nhật LastActiveAt với raw SQL để nhanh và tránh load toàn bộ User entity
                        var timeThreshold = DateTime.UtcNow.AddMinutes(-30);
                        
                        // Nếu dùng EF Core 7+ có ExecuteUpdateAsync, ở đây viết an toàn cho nhiều phiên bản
                        // "UPDATE Users SET LastActiveAt = GETUTCDATE() WHERE Id = @p0 AND LastActiveAt < @p1"
                        
                        var user = await dbContext.Users.FindAsync(userId);
                        if (user != null && user.LastActiveAt < timeThreshold)
                        {
                            user.LastActiveAt = DateTime.UtcNow;
                            await dbContext.SaveChangesAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not update LastActiveAt for user {UserId}", userId);
                    }
                }
            }
        }
    }
}
