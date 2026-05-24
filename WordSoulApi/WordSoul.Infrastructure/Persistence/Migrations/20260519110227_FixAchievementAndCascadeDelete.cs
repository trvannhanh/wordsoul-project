using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSoul.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixAchievementAndCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnswerRecords_LearningSessions_LearningSessionId",
                table: "AnswerRecords");

            migrationBuilder.AlterColumn<int>(
                name: "RewardItemId",
                table: "Achievements",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "RewardXp",
                table: "Achievements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_AnswerRecords_LearningSessions_LearningSessionId",
                table: "AnswerRecords",
                column: "LearningSessionId",
                principalTable: "LearningSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnswerRecords_LearningSessions_LearningSessionId",
                table: "AnswerRecords");

            migrationBuilder.DropColumn(
                name: "RewardXp",
                table: "Achievements");

            migrationBuilder.AlterColumn<int>(
                name: "RewardItemId",
                table: "Achievements",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AnswerRecords_LearningSessions_LearningSessionId",
                table: "AnswerRecords",
                column: "LearningSessionId",
                principalTable: "LearningSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
