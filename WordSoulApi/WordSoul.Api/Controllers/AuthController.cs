using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using WordSoul.Application.DTOs.User;
using WordSoul.Application.Interfaces.Services;

namespace WordSoul.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    [EnableCors("AllowFrontend")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _authService = authService;
            _configuration = configuration;
            _logger = logger;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            var user = await _authService.RegisterAsync(registerDto);
            if (user == null)
                return BadRequest("User registration failed.");
            return Ok(user);
        }

        // POST: api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenResponseDto>> Login(LoginDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);
            if (result is null)
                return Unauthorized("Invalid username or password.");
            return Ok(result);
        }

        // POST: api/auth/refresh-token
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenResponseDto>> RefreshToken(RefreshTokenRequestDto refreshTokenRequestDto)
        {
            var result = await _authService.RefreshTokenAsync(refreshTokenRequestDto);
            if (result is null || result.AccessToken is null || result.RefreshToken is null)
                return Unauthorized("Invalid refresh token.");
            return Ok(result);
        }

        // GET: api/auth/google-login
        /// <summary>
        /// Redirect người dùng đến trang đăng nhập Google.
        /// </summary>
        [HttpGet("google-login")]
        [AllowAnonymous]
        public IActionResult GoogleLogin([FromQuery] string? state)
        {
            var clientId     = _configuration["Google:ClientId"]!;
            var redirectUri  = Uri.EscapeDataString(_configuration["Google:RedirectUri"]!);
            var scope        = Uri.EscapeDataString("openid email profile");

            var googleAuthUrl =
                $"https://accounts.google.com/o/oauth2/v2/auth" +
                $"?client_id={clientId}" +
                $"&redirect_uri={redirectUri}" +
                $"&response_type=code" +
                $"&scope={scope}" +
                $"&access_type=offline" +
                $"&prompt=consent";

            if (!string.IsNullOrEmpty(state))
            {
                googleAuthUrl += $"&state={Uri.EscapeDataString(state)}";
            }

            return Redirect(googleAuthUrl);
        }

        // GET: api/auth/google-response
        /// <summary>
        /// Nhận authorization code từ Google, xử lý đăng nhập và redirect về frontend kèm token.
        /// </summary>
        [HttpGet("google-response")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleResponse(
            [FromQuery] string? code,
            [FromQuery] string? error,
            [FromQuery] string? state,
            CancellationToken ct = default)
        {
            var frontendUrl = _configuration["AllowedOrigins"]?.Split(",")[0]?.Trim()
                              ?? "http://localhost:5173";

            if (error != null || string.IsNullOrEmpty(code))
            {
                _logger.LogWarning("Google OAuth callback nhận error: {Error}", error);
                return Redirect($"{frontendUrl}/login?error=google_denied");
            }

            int? starterPetId = null;
            if (!string.IsNullOrEmpty(state))
            {
                var parts = state.Split('&');
                foreach (var part in parts)
                {
                    if (part.StartsWith("starterPetId="))
                    {
                        var val = part.Substring("starterPetId=".Length);
                        if (int.TryParse(val, out var id))
                        {
                            starterPetId = id;
                        }
                    }
                }
            }

            var result = await _authService.GoogleLoginAsync(code, starterPetId, ct);
            if (result == null)
            {
                return Redirect($"{frontendUrl}/login?error=google_failed");
            }

            // Redirect về frontend kèm token (frontend sẽ lưu vào cookie)
            return Redirect(
                $"{frontendUrl}/auth/callback" +
                $"?accessToken={Uri.EscapeDataString(result.AccessToken)}" +
                $"&refreshToken={Uri.EscapeDataString(result.RefreshToken)}");
        }
    }
}
