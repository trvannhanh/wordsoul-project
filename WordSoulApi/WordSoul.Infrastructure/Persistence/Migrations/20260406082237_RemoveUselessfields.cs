using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSoul.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUselessfields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuestionType",
                table: "VocabularyReviewHistories");

            migrationBuilder.DropColumn(
                name: "IsSrsEvaluated",
                table: "SessionVocabularies");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QuestionType",
                table: "VocabularyReviewHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsSrsEvaluated",
                table: "SessionVocabularies",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
