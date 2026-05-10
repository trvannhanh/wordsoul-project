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
    /// Bao gồm đăng nhập, đăng ký, làm mới Token, đăng nhập bằng Google và quản lý RefreshToken.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _uow;
        private readonly IConfiguration _configuration;
        private readonly IActivityLogService _activityLogService;
        private readonly IDailyQuestService _dailyQuestService;
        private readonly ILogger<AuthService> _logger;
        private readonly IGoogleOAuthService _googleOAuthService;

        public AuthService(
            IUnitOfWork uow,
            IConfiguration configuration,
            IActivityLogService activityLogService,
            IDailyQuestService dailyQuestService,
            ILogger<AuthService> logger,
            IGoogleOAuthService googleOAuthService)
        {
            _uow = uow;
            _configuration = configuration;
            _activityLogService = activityLogService;
            _dailyQuestService = dailyQuestService;
            _logger = logger;
            _googleOAuthService = googleOAuthService;
        }

        /// <summary>
        /// Xác thực người dùng bằng Username & Password.
        /// Trả về TokenResponseDto nếu đăng nhập thành công, ngược lại null.
        /// </summary>
        public async Task<TokenResponseDto?> LoginAsync(LoginDto loginDto, CancellationToken ct = default)
        {
            var user = await _uow.Auth.LoginUserAsync(loginDto.Username, ct);

            if (user == null || user.PasswordHash == null ||
                new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, loginDto.Password)
                == PasswordVerificationResult.Failed)
            {
                return null;
            }

            await _activityLogService.TrackUserLoginAsync(user.Id, ct);
            await _dailyQuestService.GenerateDailyQuestsForUserAsync(user.Id);

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
            await _uow.SaveChangesAsync(ct);

            await InitializeNewUserAsync(user, registerDto.StarterPetId, ct);
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
        /// Xử lý đăng nhập bằng Google OAuth (Authorization Code Flow).
        /// 1. Exchange code → Google access_token → profile
        /// 2. Tìm user theo (provider, providerKey) → đăng nhập luôn
        /// 3. Tìm user theo email → liên kết tài khoản tự động
        /// 4. Không tìm thấy → tạo user mới
        /// </summary>
        public async Task<TokenResponseDto?> GoogleLoginAsync(string code, CancellationToken ct = default)
        {
            var googleUserInfo = await _googleOAuthService.ExchangeCodeForUserInfoAsync(code, ct);
            if (googleUserInfo == null)
            {
                _logger.LogWarning("Google OAuth: Không thể lấy thông tin người dùng từ Google.");
                return null;
            }

            if (!googleUserInfo.EmailVerified)
            {
                _logger.LogWarning("Google OAuth: Email {Email} chưa được xác minh.", googleUserInfo.Email);
                return null;
            }

            const string provider = "Google";

            // Tìm user đã liên kết trước đó
            var existingUser = await _uow.Auth.GetUserByExternalLoginAsync(provider, googleUserInfo.Sub, ct);
            if (existingUser != null)
            {
                _logger.LogInformation("Google OAuth: User {UserId} đăng nhập qua Google.", existingUser.Id);
                await _activityLogService.TrackUserLoginAsync(existingUser.Id, ct);
                await _dailyQuestService.GenerateDailyQuestsForUserAsync(existingUser.Id);
                return await CreateTokenResponse(existingUser, ct);
            }

            // Tìm user theo email → liên kết tài khoản tự động
            var userByEmail = await _uow.Auth.GetUserByEmailAsync(googleUserInfo.Email, ct);
            if (userByEmail != null)
            {
                _logger.LogInformation(
                    "Google OAuth: Liên kết tài khoản Google với User {UserId} (email: {Email}).",
                    userByEmail.Id, googleUserInfo.Email);

                userByEmail.ExternalLoginProvider    = provider;
                userByEmail.ExternalLoginProviderKey = googleUserInfo.Sub;
                userByEmail.ExternalLoginEmail       = googleUserInfo.Email;

                if (string.IsNullOrEmpty(userByEmail.AvatarUrl) && !string.IsNullOrEmpty(googleUserInfo.Picture))
                    userByEmail.AvatarUrl = googleUserInfo.Picture;

                await _uow.Auth.UpdateUserAsync(userByEmail, ct);
                await _uow.SaveChangesAsync(ct);

                await _activityLogService.TrackUserLoginAsync(userByEmail.Id, ct);
                await _dailyQuestService.GenerateDailyQuestsForUserAsync(userByEmail.Id);
                return await CreateTokenResponse(userByEmail, ct);
            }

            // Tạo user mới từ Google profile
            var username = GenerateUsernameFromGoogleName(googleUserInfo.Name ?? googleUserInfo.Email);

            var newUser = new User
            {
                Username  = username,
                Email     = googleUserInfo.Email,
                PasswordHash              = null,   // Google user không có mật khẩu hệ thống
                ExternalLoginProvider    = provider,
                ExternalLoginProviderKey = googleUserInfo.Sub,
                ExternalLoginEmail       = googleUserInfo.Email,
                AvatarUrl = googleUserInfo.Picture
            };

            var createdUser = await _uow.Auth.RegisterUserAsync(newUser, ct);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("Google OAuth: Tạo tài khoản mới User {UserId} cho email {Email}.",
                createdUser.Id, googleUserInfo.Email);

            await InitializeNewUserAsync(createdUser, null, ct);
            await _activityLogService.TrackUserRegisterAsync(createdUser.Id, ct);

            return await CreateTokenResponse(createdUser, ct);
        }

        /// <summary>
        /// Tạo mới AccessToken và RefreshToken thông qua RefreshToken hiện tại.
        /// </summary>
        public async Task<TokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken ct = default)
        {
            var user = await ValidateRefreshTokenAsync(request.Id, request.RefreshToken, ct);
            if (user is null) return null;
            return await CreateTokenResponse(user, ct);
        }

        // ─────────────────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Khởi tạo dữ liệu mặc định cho user mới: Achievements, Starter Pet.
        /// Gọi sau khi user đã được persist vào DB và SaveChanges đã được gọi lần đầu.
        /// </summary>
        private async Task InitializeNewUserAsync(User user, int? starterPetId, CancellationToken ct)
        {
            // 1. Gán achievement mặc định
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

            // 2. Gán Starter Pet
            // Nếu không có StarterPetId → chọn ngẫu nhiên 1 trong 3: Bulbasaur(1), Charmander(4), Squirtle(7)
            int finalStarterPetId = starterPetId ?? new[] { 1, 4, 7 }[new Random().Next(3)];

            var starterPet = await _uow.Pet.GetPetByIdAsync(finalStarterPetId, ct);
            if (starterPet != null && starterPet.IsActive)
            {
                var userStarterPet = new UserOwnedPet
                {
                    UserId     = user.Id,
                    PetId      = starterPet.Id,
                    IsActive   = true,
                    Level      = 1,
                    Experience = 0,
                    IsFavorite = true,
                    AcquiredAt = DateTime.UtcNow
                };
                await _uow.UserOwnedPet.CreateUserOwnedPetAsync(userStarterPet, ct);
                await _uow.SaveChangesAsync(ct);
                _logger.LogInformation("Gán Starter Pet ID {PetId} ({PetName}) cho User {UserId}.",
                    starterPet.Id, starterPet.Name, user.Id);
            }
            else
            {
                _logger.LogWarning("StarterPetId {PetId} không tìm thấy hoặc không active — bỏ qua.", finalStarterPetId);
            }
        }

        /// <summary>
        /// Sinh username hợp lệ từ display name của Google.
        /// VD: "Nguyễn Văn A" → "nguyen_van_a_4f2a1b"
        /// </summary>
        private static string GenerateUsernameFromGoogleName(string name)
        {
            var normalized = name
                .Normalize(System.Text.NormalizationForm.FormD)
                .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                            != System.Globalization.UnicodeCategory.NonSpacingMark)
                .Select(c => char.IsLetterOrDigit(c) ? char.ToLower(c) : '_')
                .ToArray();

            var slug = new string(normalized).Trim('_');
            while (slug.Contains("__")) slug = slug.Replace("__", "_");

            var suffix = Guid.NewGuid().ToString("N")[..6];
            var combined = $"{slug}_{suffix}";
            return combined.Length > 97 ? combined[..97] : combined;
        }

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

        private async Task<TokenResponseDto> CreateTokenResponse(User user, CancellationToken ct)
        {
            return new TokenResponseDto
            {
                AccessToken  = CreateToken(user),
                RefreshToken = await GenerateAndSaveRefreshTokenAsync(user, ct)
            };
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user, CancellationToken ct)
        {
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _uow.Auth.UpdateUserAsync(user, ct);
            await _uow.SaveChangesAsync(ct);

            return refreshToken;
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username ?? user.Email),
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
