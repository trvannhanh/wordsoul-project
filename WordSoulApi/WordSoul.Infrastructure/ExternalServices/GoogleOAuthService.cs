using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using WordSoul.Application.DTOs.User;
using WordSoul.Application.Interfaces.Services;

namespace WordSoul.Infrastructure.ExternalServices
{
    /// <summary>
    /// Implementation của IGoogleOAuthService — giao tiếp với Google OAuth2 API
    /// để exchange Authorization Code lấy thông tin người dùng.
    /// </summary>
    public class GoogleOAuthService : IGoogleOAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleOAuthService> _logger;

        public GoogleOAuthService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<GoogleOAuthService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<GoogleUserInfoDto?> ExchangeCodeForUserInfoAsync(string code, CancellationToken ct = default)
        {
            try
            {
                var clientId     = _configuration["Google:ClientId"]!;
                var clientSecret = _configuration["Google:ClientSecret"]!;
                var redirectUri  = _configuration["Google:RedirectUri"]!;

                var httpClient = _httpClientFactory.CreateClient();

                // Step 1: Exchange Authorization Code → Google Access Token
                var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["code"]          = code,
                    ["client_id"]     = clientId,
                    ["client_secret"] = clientSecret,
                    ["redirect_uri"]  = redirectUri,
                    ["grant_type"]    = "authorization_code"
                });

                var tokenResponse = await httpClient.PostAsync(
                    "https://oauth2.googleapis.com/token", tokenRequest, ct);

                if (!tokenResponse.IsSuccessStatusCode)
                {
                    var err = await tokenResponse.Content.ReadAsStringAsync(ct);
                    _logger.LogError("Google token exchange thất bại (HTTP {Status}): {Error}",
                        (int)tokenResponse.StatusCode, err);
                    return null;
                }

                var tokenJson = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);

                if (!tokenJson.TryGetProperty("access_token", out var atProp))
                {
                    _logger.LogError("Google token response không có access_token.");
                    return null;
                }

                var accessToken = atProp.GetString();
                if (string.IsNullOrEmpty(accessToken))
                    return null;

                // Step 2: Dùng Access Token để lấy thông tin user
                var userInfoClient = _httpClientFactory.CreateClient();
                userInfoClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var userInfoResponse = await userInfoClient.GetAsync(
                    "https://www.googleapis.com/oauth2/v3/userinfo", ct);

                if (!userInfoResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Lấy Google userinfo thất bại (HTTP {Status}).",
                        (int)userInfoResponse.StatusCode);
                    return null;
                }

                return await userInfoResponse.Content
                    .ReadFromJsonAsync<GoogleUserInfoDto>(cancellationToken: ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi không mong đợi khi exchange Google OAuth code.");
                return null;
            }
        }
    }
}
