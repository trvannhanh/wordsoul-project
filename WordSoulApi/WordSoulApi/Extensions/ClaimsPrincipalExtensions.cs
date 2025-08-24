using System.Security.Claims;

namespace WordSoulApi.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return string.IsNullOrEmpty(userId) ? 0 : int.Parse(userId);
        }

        public static string GetUsername(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        }

        public static string GetUserRole(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }
    }
}
