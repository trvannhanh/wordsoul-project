using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSoul.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Method = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    StatusCode = table.Column<int>(type: "int", nullable: false),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    RequestPayload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponsePayload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemLogs", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SystemConfigurations",
                columns: new[] { "Key", "Category", "DataType", "Description", "LastUpdatedAt", "LastUpdatedBy", "Value" },
                values: new object[] { "LogRetentionDays", "SYSTEM", "Integer", "Number of days to keep system logs before auto-deleting", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "7" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemLogs");

            migrationBuilder.DeleteData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "LogRetentionDays");
        }
    }
}
