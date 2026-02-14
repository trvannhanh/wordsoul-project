using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WordSoul.Application.DTOs.User;
using WordSoul.Application.Interfaces;
using WordSoul.Application.Interfaces.Services;
using WordSoul.Domain.Entities;

namespace WordSoul.Application.Services
{
    /// <summary>
    /// Cung cấp các chức năng xác thực và quản lý Token của người dùng.
    /// Bao gồm đăng nhập, đăng ký, làm mới Token và quản lý RefreshToken.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _uow;
        private readonly IConfiguration _configuration;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUnitOfWork uow,
            IConfiguration configuration,
            IActivityLogService activityLogService,
            ILogger<AuthService> logger)
        {
            _uow = uow;
            _configuration = configuration;
            _activityLogService = activityLogService;
            _logger = logger;
        }

        /// <summary>
        /// Xác thực người dùng bằng Username & Password.
        /// Trả về TokenResponseDto nếu đăng nhập thành công, ngược lại null.
        /// </summary>
        public async Task<TokenResponseDto?> LoginAsync(LoginDto loginDto, CancellationToken ct = default)
        {
            var user = await _uow.Auth.LoginUserAsync(loginDto.Username, ct);

            if (user == null ||
                new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, loginDto.Password)
                == PasswordVerificationResult.Failed)
            {
                return null;
            }

            await _activityLogService.TrackUserLoginAsync(user.Id, ct);

            return await CreateTokenResponse(user, ct);
        }

        /// <summary>
        /// Đăng ký tài khoản mới. Trả về UserDto nếu thành công, ngược lại null.
        /// </summary>
        public async Task<UserDto?> RegisterAsync(RegisterDto registerDto, CancellationToken ct = default)
        {
            if (await _uow.Auth.UserExistsAsync(registerDto.Username, ct))
                return null;

            if (await _uow.Auth.EmailExistsAsync(registerDto.Email, ct))
                return null;

            var newUser = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = new PasswordHasher<User>().HashPassword(null!, registerDto.Password)
            };

            var user = await _uow.Auth.RegisterUserAsync(newUser, ct);

            // Gán achievement mặc định cho user
            var achievements = await _uow.Achievement.GetAchievementsAsync(null, 1, 10, ct);

            var userAchievements = achievements
                .Select(a => new UserAchievement
                {
                    UserId = user.Id,
                    AchievementId = a.Id,
                    ProgressValue = 0,
                    IsCompleted = false
                })
                .ToList();

            await _uow.UserAchievement.BulkCreateUserAchievementAsync(userAchievements, ct);
            await _uow.SaveChangesAsync(ct);


            await _activityLogService.TrackUserRegisterAsync(user.Id, ct);

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };
        }

        /// <summary>
        /// Tạo mới AccessToken và RefreshToken thông qua RefreshToken hiện tại.
        /// </summary>
        public async Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken ct = default)
        {
            var user = await ValidateRefreshTokenAsync(request.Id, request.RefreshToken, ct);

            if (user is null)
                return null;

            return await CreateTokenResponse(user, ct);
        }

        /// <summary>
        /// Xác thực RefreshToken của người dùng.
        /// Trả về User nếu hợp lệ, ngược lại null.
        /// </summary>
        private async Task<User?> ValidateRefreshTokenAsync(int userId, string refreshToken, CancellationToken ct)
        {
            var user = await _uow.Auth.GetUserByIdAsync(userId, ct);

            if (user is null ||
                user.RefreshToken != refreshToken ||
                user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return null;
            }

            return user;
        }

        /// <summary>
        /// Tạo TokenResponse (AccessToken + RefreshToken).
        /// </summary>
        private async Task<TokenResponseDto> CreateTokenResponse(User user, CancellationToken ct)
        {
            return new TokenResponseDto
            {
                AccessToken = CreateToken(user),
                RefreshToken = await GenerateAndSaveRefreshTokenAsync(user, ct)
            };
        }

        /// <summary>
        /// Sinh một RefreshToken ngẫu nhiên (32 bytes).
        /// </summary>
        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        /// <summary>
        /// Tạo và lưu RefreshToken mới vào DB.
        /// </summary>
        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user, CancellationToken ct)
        {
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _uow.Auth.UpdateUserAsync(user, ct);
            await _uow.SaveChangesAsync(ct);

            return refreshToken;
        }

        /// <summary>
        /// Tạo JWT Token cho người dùng dựa vào Claim.
        /// </summary>
        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name , user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["AppSettings:Token"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var jwt = new JwtSecurityToken(
                issuer: _configuration["AppSettings:Issuer"],
                audience: _configuration["AppSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}
