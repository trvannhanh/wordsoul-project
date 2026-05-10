using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSoul.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalLoginFieldsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<string>(
                name: "ExternalLoginEmail",
                table: "Users",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalLoginProvider",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalLoginProviderKey",
                table: "Users",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ExternalLoginProvider_ExternalLoginProviderKey",
                table: "Users",
                columns: new[] { "ExternalLoginProvider", "ExternalLoginProviderKey" },
                unique: true,
                filter: "[ExternalLoginProvider] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_ExternalLoginProvider_ExternalLoginProviderKey",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ExternalLoginEmail",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ExternalLoginProvider",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ExternalLoginProviderKey",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }
    }
}
