using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSoulApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAnswerRecordUniqueKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AnswerRecords_UserId_LearningSessionId_VocabularyId",
                table: "AnswerRecords");

            migrationBuilder.CreateIndex(
                name: "IX_AnswerRecords_UserId_LearningSessionId_VocabularyId_QuestionType",
                table: "AnswerRecords",
                columns: new[] { "UserId", "LearningSessionId", "VocabularyId", "QuestionType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AnswerRecords_UserId_LearningSessionId_VocabularyId_QuestionType",
                table: "AnswerRecords");

            migrationBuilder.CreateIndex(
                name: "IX_AnswerRecords_UserId_LearningSessionId_VocabularyId",
                table: "AnswerRecords",
                columns: new[] { "UserId", "LearningSessionId", "VocabularyId" },
                unique: true);
        }
    }
}
