using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSoul.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAnswerRecordUniqueContraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AnswerRecords_LearningSessionId_VocabularyId_QuestionType",
                table: "AnswerRecords");

            migrationBuilder.CreateIndex(
                name: "IX_AnswerRecords_LearningSessionId_VocabularyId_QuestionType",
                table: "AnswerRecords",
                columns: new[] { "LearningSessionId", "VocabularyId", "QuestionType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AnswerRecords_LearningSessionId_VocabularyId_QuestionType",
                table: "AnswerRecords");

            migrationBuilder.CreateIndex(
                name: "IX_AnswerRecords_LearningSessionId_VocabularyId_QuestionType",
                table: "AnswerRecords",
                columns: new[] { "LearningSessionId", "VocabularyId", "QuestionType" },
                unique: true);
        }
    }
}
