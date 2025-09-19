using System.Security.Claims;

namespace WordSoulApi.Extensions
{

    // Phương thức mở rộng để lấy thông tin người dùng từ ClaimsPrincipal
    public static class ClaimsPrincipalExtensions
    {
        // Lấy UserId từ ClaimsPrincipal, trả về 0 nếu không tìm thấy hoặc không hợp lệ
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return string.IsNullOrEmpty(userId) ? 0 : int.Parse(userId);
        }

        // Lấy Username từ ClaimsPrincipal, trả về chuỗi rỗng nếu không tìm thấy
        public static string GetUsername(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        }

        // Lấy UserRole từ ClaimsPrincipal, trả về chuỗi rỗng nếu không tìm thấy
        public static string GetUserRole(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }
    }
}
