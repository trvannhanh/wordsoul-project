using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSoul.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPronunciationPractice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastPronunciationAt",
                table: "UserVocabularyProgresses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PronunciationPerfectStreak",
                table: "UserVocabularyProgresses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PronunciationWrongCount",
                table: "UserVocabularyProgresses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PronunciationAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    VocabularyId = table.Column<int>(type: "int", nullable: false),
                    AttemptTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AccuracyScore = table.Column<double>(type: "float", nullable: false),
                    FluencyScore = table.Column<double>(type: "float", nullable: false),
                    CompletenessScore = table.Column<double>(type: "float", nullable: false),
                    PronunciationScore = table.Column<double>(type: "float", nullable: false),
                    Result = table.Column<int>(type: "int", nullable: false),
                    AzureRawResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsecutivePerfectCount = table.Column<int>(type: "int", nullable: false),
                    XpAwarded = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PronunciationAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PronunciationAttempts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PronunciationAttempts_Vocabularies_VocabularyId",
                        column: x => x.VocabularyId,
                        principalTable: "Vocabularies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PronunciationAttempts_UserId_Result",
                table: "PronunciationAttempts",
                columns: new[] { "UserId", "Result" });

            migrationBuilder.CreateIndex(
                name: "IX_PronunciationAttempts_UserId_VocabularyId_AttemptTime",
                table: "PronunciationAttempts",
                columns: new[] { "UserId", "VocabularyId", "AttemptTime" });

            migrationBuilder.CreateIndex(
                name: "IX_PronunciationAttempts_VocabularyId",
                table: "PronunciationAttempts",
                column: "VocabularyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PronunciationAttempts");

            migrationBuilder.DropColumn(
                name: "LastPronunciationAt",
                table: "UserVocabularyProgresses");

            migrationBuilder.DropColumn(
                name: "PronunciationPerfectStreak",
                table: "UserVocabularyProgresses");

            migrationBuilder.DropColumn(
                name: "PronunciationWrongCount",
                table: "UserVocabularyProgresses");
        }
    }
}
