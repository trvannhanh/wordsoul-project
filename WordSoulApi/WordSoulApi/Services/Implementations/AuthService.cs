using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WordSoulApi.Models.DTOs.User;
using WordSoulApi.Models.Entities;
using WordSoulApi.Repositories.Interfaces;
using WordSoulApi.Services.Interfaces;

namespace WordSoulApi.Services.Implementations
{
    public class AuthService : IAuthService
    {
        // Tiêm các phụ thuộc cần thiết
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;
        private readonly IActivityLogService _activityLogService;
        private readonly IUserAchievementRepository _userAchievementRepository;
        private readonly IAchievementRepository _achievementRepository;

        public AuthService(IAuthRepository authRepository, IConfiguration configuration, 
                            IActivityLogService activityLogService, IAchievementRepository achievementRepository,
                            IUserAchievementRepository userAchievementRepository)
        {
            _authRepository = authRepository;
            _configuration = configuration;
            _activityLogService = activityLogService;
            _userAchievementRepository = userAchievementRepository;
            _achievementRepository = achievementRepository;
        }

        // Đăng nhập người dùng và trả về TokenResponseDto nếu thành công, ngược lại trả về null
        public async Task<TokenResponseDto?> LoginAsync(LoginDto loginDto)
        {
            var user = await _authRepository.LoginUserAsync(loginDto.Username);
            if (user == null || new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, loginDto.Password)
                == PasswordVerificationResult.Failed)
            {
                return null!;
            }

            await _activityLogService.CreateActivityLogAsync(user.Id, "Login", "User logged in");

            return await CreateTokenResponse(user);

        }

        // Tạo phản hồi token bao gồm AccessToken và RefreshToken
        private async Task<TokenResponseDto> CreateTokenResponse(User user)
        {
            return new TokenResponseDto
            {
                AccessToken = CreateToken(user),
                RefreshToken = await GenerateAndSaveRefreshTokenAsync(user)
            };
        }

        // Đăng ký người dùng mới và trả về User nếu thành công, ngược lại trả về null
        public async Task<UserDto> RegisterAsync(RegisterDto registerDto)
        {
            if (await _authRepository.UserExistsAsync(registerDto.Username))
            {
                return null!; // Trả về null thay vì ném exception
            }
            if (await _authRepository.EmailExistsAsync(registerDto.Email))
            {
                return null!;
            }
            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = new PasswordHasher<User>().HashPassword(null!, registerDto.Password)
            };

            var registeredUser = await _authRepository.RegisterUserAsync(user);

            var allAchievements = await _achievementRepository.GetAchievementsAsync(null, 1, 10);
            var userAchievements = allAchievements.Select(a => new UserAchievement
            {
                UserId = registeredUser.Id,
                AchievementId = a.Id,
                ProgressValue = 0,
                IsCompleted = false,
                CompletedAt = null
            }).ToList();

            await _userAchievementRepository.BulkCreateUserAchievementAsync(userAchievements);

            return new UserDto
            {
                Id = registeredUser.Id,
                Username = registeredUser.Username,
                Email = registeredUser.Email,
                Role = registeredUser.Role.ToString(),
                CreatedAt = registeredUser.CreatedAt,
                IsActive = registeredUser.IsActive
            };
        }

        // Làm mới token sử dụng RefreshToken và trả về TokenResponseDto nếu thành công, ngược lại trả về null
        public async Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var user = await ValidateRefreshTokenAsync(request.Id, request.RefreshToken);
            if (user is null)
                return null;

            return await CreateTokenResponse(user);
        }

        // Xác thực RefreshToken
        private async Task<User?> ValidateRefreshTokenAsync(int userId, string refreshToken)
        {
            var user = await _authRepository.GetUserByIdAsync(userId);
            if(user is null || user.RefreshToken != refreshToken
                || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return null;
            }

            return user;
        }
        // Tạo mã làm mới token
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        // Tạo và lưu mã làm mới token vào cơ sở dữ liệu
        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _authRepository.UpdateUserAsync(user);
            return refreshToken;
        }

        // Tạo JWT token dựa trên thông tin người dùng
        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name , user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration.GetValue<string>("AppSettings:Token")!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration.GetValue<string>("AppSettings:Issuer"),
                audience: _configuration.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        }
    }
}
