using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSoulApi.Migrations
{
    /// <inheritdoc />
    public partial class TotalCompletedSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AnswerRecords_UserId",
                table: "AnswerRecords");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "VocabularySets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "totalCompletedSession",
                table: "UserVocabularySets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "NextReviewTime",
                table: "UserVocabularyProgresses",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdated",
                table: "UserVocabularyProgresses",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "XP",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "VocabularySetId",
                table: "LearningSessions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnswerRecords_UserId_LearningSessionId_QuizQuestionId",
                table: "AnswerRecords",
                columns: new[] { "UserId", "LearningSessionId", "QuizQuestionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AnswerRecords_UserId_LearningSessionId_QuizQuestionId",
                table: "AnswerRecords");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "VocabularySets");

            migrationBuilder.DropColumn(
                name: "totalCompletedSession",
                table: "UserVocabularySets");

            migrationBuilder.DropColumn(
                name: "XP",
                table: "Users");

            migrationBuilder.AlterColumn<DateTime>(
                name: "NextReviewTime",
                table: "UserVocabularyProgresses",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastUpdated",
                table: "UserVocabularyProgresses",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<int>(
                name: "VocabularySetId",
                table: "LearningSessions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_AnswerRecords_UserId",
                table: "AnswerRecords",
                column: "UserId");
        }
    }
}
