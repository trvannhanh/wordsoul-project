using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSoulApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveQuizQuestionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnswerRecords_QuizQuestions_QuizQuestionId",
                table: "AnswerRecords");

            migrationBuilder.DropTable(
                name: "QuizQuestions");

            migrationBuilder.RenameColumn(
                name: "QuizQuestionId",
                table: "AnswerRecords",
                newName: "VocabularyId");

            migrationBuilder.RenameIndex(
                name: "IX_AnswerRecords_UserId_LearningSessionId_QuizQuestionId",
                table: "AnswerRecords",
                newName: "IX_AnswerRecords_UserId_LearningSessionId_VocabularyId");

            migrationBuilder.RenameIndex(
                name: "IX_AnswerRecords_QuizQuestionId",
                table: "AnswerRecords",
                newName: "IX_AnswerRecords_VocabularyId");

            migrationBuilder.AddColumn<int>(
                name: "QuestionType",
                table: "AnswerRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_AnswerRecords_Vocabularies_VocabularyId",
                table: "AnswerRecords",
                column: "VocabularyId",
                principalTable: "Vocabularies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnswerRecords_Vocabularies_VocabularyId",
                table: "AnswerRecords");

            migrationBuilder.DropColumn(
                name: "QuestionType",
                table: "AnswerRecords");

            migrationBuilder.RenameColumn(
                name: "VocabularyId",
                table: "AnswerRecords",
                newName: "QuizQuestionId");

            migrationBuilder.RenameIndex(
                name: "IX_AnswerRecords_VocabularyId",
                table: "AnswerRecords",
                newName: "IX_AnswerRecords_QuizQuestionId");

            migrationBuilder.RenameIndex(
                name: "IX_AnswerRecords_UserId_LearningSessionId_VocabularyId",
                table: "AnswerRecords",
                newName: "IX_AnswerRecords_UserId_LearningSessionId_QuizQuestionId");

            migrationBuilder.CreateTable(
                name: "QuizQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VocabularyId = table.Column<int>(type: "int", nullable: false),
                    CorrectAnswer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Options = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prompt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuestionType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizQuestions_Vocabularies_VocabularyId",
                        column: x => x.VocabularyId,
                        principalTable: "Vocabularies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuizQuestions_VocabularyId",
                table: "QuizQuestions",
                column: "VocabularyId");

            migrationBuilder.AddForeignKey(
                name: "FK_AnswerRecords_QuizQuestions_QuizQuestionId",
                table: "AnswerRecords",
                column: "QuizQuestionId",
                principalTable: "QuizQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
