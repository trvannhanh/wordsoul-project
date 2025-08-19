using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSoulApi.Migrations
{
    /// <inheritdoc />
    public partial class AnswerRecordNNSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Prompt",
                table: "QuizQuestions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "QuizQuestions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<int>(
                name: "VocabularySetId",
                table: "LearningSessions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "LearningSessionId",
                table: "AnswerRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AnswerRecords_LearningSessionId",
                table: "AnswerRecords",
                column: "LearningSessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AnswerRecords_LearningSessions_LearningSessionId",
                table: "AnswerRecords",
                column: "LearningSessionId",
                principalTable: "LearningSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnswerRecords_LearningSessions_LearningSessionId",
                table: "AnswerRecords");

            migrationBuilder.DropIndex(
                name: "IX_AnswerRecords_LearningSessionId",
                table: "AnswerRecords");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "QuizQuestions");

            migrationBuilder.DropColumn(
                name: "LearningSessionId",
                table: "AnswerRecords");

            migrationBuilder.AlterColumn<string>(
                name: "Prompt",
                table: "QuizQuestions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "VocabularySetId",
                table: "LearningSessions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
