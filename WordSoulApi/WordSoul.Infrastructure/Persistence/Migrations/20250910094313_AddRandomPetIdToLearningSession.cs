using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSoul.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRandomPetIdToLearningSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PetId",
                table: "LearningSessions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LearningSessions_PetId",
                table: "LearningSessions",
                column: "PetId");

            migrationBuilder.AddForeignKey(
                name: "FK_LearningSessions_Pets_PetId",
                table: "LearningSessions",
                column: "PetId",
                principalTable: "Pets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LearningSessions_Pets_PetId",
                table: "LearningSessions");

            migrationBuilder.DropIndex(
                name: "IX_LearningSessions_PetId",
                table: "LearningSessions");

            migrationBuilder.DropColumn(
                name: "PetId",
                table: "LearningSessions");
        }
    }
}
