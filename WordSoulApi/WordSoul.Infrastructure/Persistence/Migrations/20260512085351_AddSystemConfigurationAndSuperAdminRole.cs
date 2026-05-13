using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WordSoul.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemConfigurationAndSuperAdminRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemConfigurations",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemConfigurations", x => x.Key);
                });

            migrationBuilder.InsertData(
                table: "SystemConfigurations",
                columns: new[] { "Key", "DataType", "Description", "LastUpdatedAt", "LastUpdatedBy", "Value" },
                values: new object[,]
                {
                    { "CatchRateWrongPenalty", "Float", "Penalty applied to catch rate for each wrong answer (e.g. 0.05 = 5%)", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "0.05" },
                    { "SrsInitialInterval1", "Integer", "First interval (days) for SM-2", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "1" },
                    { "SrsInitialInterval2", "Integer", "Second interval (days) for SM-2", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "6" },
                    { "SrsMinEf", "Float", "Minimum Ease Factor for SM-2 Algorithm", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "1.3" },
                    { "XpRewardNewSession", "Integer", "XP rewarded for completing a learning session with new words", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "20" },
                    { "XpRewardReviewSession", "Integer", "XP rewarded for completing a review session", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "100" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemConfigurations");
        }
    }
}
