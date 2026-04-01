using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSoul.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase1_BattleEnhancement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "GymLeaderPets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "TypeMultiplier",
                table: "BattleRounds",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "PetType",
                table: "BattlePetStates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SecondaryPetType",
                table: "BattlePetStates",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Level",
                table: "GymLeaderPets");

            migrationBuilder.DropColumn(
                name: "TypeMultiplier",
                table: "BattleRounds");

            migrationBuilder.DropColumn(
                name: "PetType",
                table: "BattlePetStates");

            migrationBuilder.DropColumn(
                name: "SecondaryPetType",
                table: "BattlePetStates");
        }
    }
}
