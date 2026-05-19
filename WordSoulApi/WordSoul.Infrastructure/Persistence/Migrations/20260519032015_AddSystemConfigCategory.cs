using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WordSoul.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemConfigCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "SystemConfigurations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "CatchRateWrongPenalty",
                column: "Category",
                value: "GAME_BALANCE");

            migrationBuilder.UpdateData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "SrsInitialInterval1",
                column: "Category",
                value: "SRS");

            migrationBuilder.UpdateData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "SrsInitialInterval2",
                column: "Category",
                value: "SRS");

            migrationBuilder.UpdateData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "SrsMinEf",
                column: "Category",
                value: "SRS");

            migrationBuilder.UpdateData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "XpRewardNewSession",
                column: "Category",
                value: "GAME_BALANCE");

            migrationBuilder.UpdateData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "XpRewardReviewSession",
                column: "Category",
                value: "GAME_BALANCE");

            migrationBuilder.InsertData(
                table: "SystemConfigurations",
                columns: new[] { "Key", "Category", "DataType", "Description", "LastUpdatedAt", "LastUpdatedBy", "Value" },
                values: new object[,]
                {
                    { "AllowRegistration", "GENERAL", "Boolean", "Allow new users to register on the platform", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "true" },
                    { "AppDisplayName", "GENERAL", "String", "Application display name shown to users in the UI", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "VocaMon" },
                    { "MaintenanceMode", "GENERAL", "Boolean", "Show maintenance notice to regular users (does not affect admins)", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "false" },
                    { "MaxGroupSize", "GENERAL", "Integer", "Maximum number of members allowed in a single user group", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "50" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "AllowRegistration");

            migrationBuilder.DeleteData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "AppDisplayName");

            migrationBuilder.DeleteData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "MaintenanceMode");

            migrationBuilder.DeleteData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "MaxGroupSize");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "SystemConfigurations");
        }
    }
}
