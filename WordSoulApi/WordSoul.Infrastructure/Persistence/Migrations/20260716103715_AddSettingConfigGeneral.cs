using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WordSoul.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSettingConfigGeneral : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "SystemConfigurations",
                columns: new[] { "Key", "Category", "DataType", "Description", "LastUpdatedAt", "LastUpdatedBy", "Value" },
                values: new object[,]
                {
                    { "AdminAppLogo", "GENERAL", "String", "Logo URL for the Admin portal", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "https://res.cloudinary.com/dqpkxxzaf/image/upload/v1759222012/egg-logo_pflvdz.png" },
                    { "AdminAppName", "GENERAL", "String", "Application name for the Admin portal", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "VocaMon Admin" },
                    { "AllowGoogleLogin", "GENERAL", "Boolean", "Enable or disable Google OAuth registration and login", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "true" },
                    { "ContactEmail", "GENERAL", "String", "Support/contact email address shown to users", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "support@vocamon.online" },
                    { "FacebookUrl", "GENERAL", "String", "Official Facebook Fanpage link", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "https://www.facebook.com/giidavibe/" },
                    { "FooterCopyright", "GENERAL", "String", "Copyright text shown in the Web App footer", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "© 2026 VocaMon. All rights reserved." },
                    { "WebAppFavicon", "GENERAL", "String", "Favicon URL for the client Web App", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "https://res.cloudinary.com/dqpkxxzaf/image/upload/v1759222012/egg-logo_pflvdz.png" },
                    { "WebAppLogo", "GENERAL", "String", "Logo URL for the client Web App", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "https://res.cloudinary.com/dqpkxxzaf/image/upload/v1759222012/egg-logo_pflvdz.png" },
                    { "WebAppName", "GENERAL", "String", "Application name for the client Web App", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "VocaMon" },
                    { "WebAppSubtitle", "GENERAL", "String", "Subtitle/Slogan for the client Web App", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "Học từ vựng cùng thú cưng" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "AdminAppLogo");

            migrationBuilder.DeleteData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "AdminAppName");

            migrationBuilder.DeleteData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "AllowGoogleLogin");

            migrationBuilder.DeleteData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "ContactEmail");

            migrationBuilder.DeleteData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "FacebookUrl");

            migrationBuilder.DeleteData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "FooterCopyright");

            migrationBuilder.DeleteData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "WebAppFavicon");

            migrationBuilder.DeleteData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "WebAppLogo");

            migrationBuilder.DeleteData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "WebAppName");

            migrationBuilder.DeleteData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "WebAppSubtitle");
        }
    }
}
