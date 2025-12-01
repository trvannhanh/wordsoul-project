using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSoul.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserIDForeignKeyFromAnswerRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnswerRecords_Users_UserId",
                table: "AnswerRecords");

            migrationBuilder.DropIndex(
                name: "IX_AnswerRecords_LearningSessionId",
                table: "AnswerRecords");

            migrationBuilder.DropIndex(
                name: "IX_AnswerRecords_UserId_LearningSessionId_VocabularyId_QuestionType",
                table: "AnswerRecords");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "AnswerRecords");

            migrationBuilder.CreateIndex(
                name: "IX_AnswerRecords_LearningSessionId_VocabularyId_QuestionType",
                table: "AnswerRecords",
                columns: new[] { "LearningSessionId", "VocabularyId", "QuestionType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AnswerRecords_LearningSessionId_VocabularyId_QuestionType",
                table: "AnswerRecords");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "AnswerRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AnswerRecords_LearningSessionId",
                table: "AnswerRecords",
                column: "LearningSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AnswerRecords_UserId_LearningSessionId_VocabularyId_QuestionType",
                table: "AnswerRecords",
                columns: new[] { "UserId", "LearningSessionId", "VocabularyId", "QuestionType" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AnswerRecords_Users_UserId",
                table: "AnswerRecords",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
