namespace WordSoul.Application.DTOs.User
{
    public class RegisterDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        /// <summary>
        /// ID của Starter Pet mà người dùng chọn trong luồng Onboarding. 
        /// Nếu null (đăng ký thông thường) thì không gán Pet.
        /// </summary>
        public int? StarterPetId { get; set; }
    }
}
