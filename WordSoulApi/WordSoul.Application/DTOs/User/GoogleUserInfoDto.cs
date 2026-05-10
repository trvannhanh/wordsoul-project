using System.Text.Json.Serialization;

namespace WordSoul.Application.DTOs.User
{
    /// <summary>
    /// DTO nội bộ — map JSON trả về từ Google userinfo endpoint
    /// (https://www.googleapis.com/oauth2/v3/userinfo)
    /// </summary>
    public class GoogleUserInfoDto
    {
        /// <summary>Google unique subject ID (dùng làm ProviderKey)</summary>
        [JsonPropertyName("sub")]
        public string Sub { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("email_verified")]
        public bool EmailVerified { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("given_name")]
        public string? GivenName { get; set; }

        [JsonPropertyName("picture")]
        public string? Picture { get; set; }
    }
}
