using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSoul.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase2_PvP_ELO : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Users",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PvpLosses",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PvpRating",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PvpWins",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "P1ConnectionId",
                table: "BattleSessions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "P1Ready",
                table: "BattleSessions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "P2ConnectionId",
                table: "BattleSessions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "P2Ready",
                table: "BattleSessions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RoomCode",
                table: "BattleSessions",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PvpLosses",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PvpRating",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PvpWins",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "P1ConnectionId",
                table: "BattleSessions");

            migrationBuilder.DropColumn(
                name: "P1Ready",
                table: "BattleSessions");

            migrationBuilder.DropColumn(
                name: "P2ConnectionId",
                table: "BattleSessions");

            migrationBuilder.DropColumn(
                name: "P2Ready",
                table: "BattleSessions");

            migrationBuilder.DropColumn(
                name: "RoomCode",
                table: "BattleSessions");
        }
    }
}
