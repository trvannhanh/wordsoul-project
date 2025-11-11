using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSoulApi.Migrations
{
    /// <inheritdoc />
    public partial class SPPetEvolution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Experience",
                table: "UserOwnedPets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "UserOwnedPets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BaseFormID",
                table: "Pets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NextEvolutionId",
                table: "Pets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RequiredLevel",
                table: "Pets",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Experience",
                table: "UserOwnedPets");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "UserOwnedPets");

            migrationBuilder.DropColumn(
                name: "BaseFormID",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "NextEvolutionId",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "RequiredLevel",
                table: "Pets");
        }
    }
}
