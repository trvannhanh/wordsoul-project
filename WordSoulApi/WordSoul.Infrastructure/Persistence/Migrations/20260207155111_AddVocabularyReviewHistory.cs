using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSoul.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVocabularyReviewHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProficiencyLevel",
                table: "UserVocabularyProgresses");

            migrationBuilder.AddColumn<bool>(
                name: "IsSrsEvaluated",
                table: "SessionVocabularies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "VocabularyReviewHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    VocabularyId = table.Column<int>(type: "int", nullable: false),
                    ReviewTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    ResponseTimeSeconds = table.Column<double>(type: "float", nullable: false),
                    HintCount = table.Column<int>(type: "int", nullable: false),
                    QuestionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Grade = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VocabularyReviewHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VocabularyReviewHistories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VocabularyReviewHistories_Vocabularies_VocabularyId",
                        column: x => x.VocabularyId,
                        principalTable: "Vocabularies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyReviewHistories_UserId",
                table: "VocabularyReviewHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyReviewHistories_VocabularyId",
                table: "VocabularyReviewHistories",
                column: "VocabularyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VocabularyReviewHistories");

            migrationBuilder.DropColumn(
                name: "IsSrsEvaluated",
                table: "SessionVocabularies");

            migrationBuilder.AddColumn<int>(
                name: "ProficiencyLevel",
                table: "UserVocabularyProgresses",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
