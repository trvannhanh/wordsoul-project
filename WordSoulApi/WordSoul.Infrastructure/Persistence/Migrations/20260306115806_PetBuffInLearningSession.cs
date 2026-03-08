using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSoul.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PetBuffInLearningSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BuffDescription",
                table: "LearningSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuffIcon",
                table: "LearningSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuffName",
                table: "LearningSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BuffPetId",
                table: "LearningSessions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PetCatchBonus",
                table: "LearningSessions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "PetHintShield",
                table: "LearningSessions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PetReducePenalty",
                table: "LearningSessions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "PetXpMultiplier",
                table: "LearningSessions",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuffDescription",
                table: "LearningSessions");

            migrationBuilder.DropColumn(
                name: "BuffIcon",
                table: "LearningSessions");

            migrationBuilder.DropColumn(
                name: "BuffName",
                table: "LearningSessions");

            migrationBuilder.DropColumn(
                name: "BuffPetId",
                table: "LearningSessions");

            migrationBuilder.DropColumn(
                name: "PetCatchBonus",
                table: "LearningSessions");

            migrationBuilder.DropColumn(
                name: "PetHintShield",
                table: "LearningSessions");

            migrationBuilder.DropColumn(
                name: "PetReducePenalty",
                table: "LearningSessions");

            migrationBuilder.DropColumn(
                name: "PetXpMultiplier",
                table: "LearningSessions");
        }
    }
}
