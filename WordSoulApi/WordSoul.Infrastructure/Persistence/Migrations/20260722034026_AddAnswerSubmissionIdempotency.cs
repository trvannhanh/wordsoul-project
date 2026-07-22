using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSoul.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAnswerSubmissionIdempotency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVocabularyCompleted",
                table: "AnswerRecords",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ResultingLevel",
                table: "AnswerRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "SubmissionId",
                table: "AnswerRecords",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // Existing rows all receive Guid.Empty from AddColumn. Backfill each row
            // before creating the unique index so production data can migrate safely.
            migrationBuilder.Sql(
                "UPDATE [AnswerRecords] SET [SubmissionId] = NEWID() " +
                "WHERE [SubmissionId] = '00000000-0000-0000-0000-000000000000';");

            migrationBuilder.CreateIndex(
                name: "IX_AnswerRecords_LearningSessionId_SubmissionId",
                table: "AnswerRecords",
                columns: new[] { "LearningSessionId", "SubmissionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AnswerRecords_LearningSessionId_SubmissionId",
                table: "AnswerRecords");

            migrationBuilder.DropColumn(
                name: "IsVocabularyCompleted",
                table: "AnswerRecords");

            migrationBuilder.DropColumn(
                name: "ResultingLevel",
                table: "AnswerRecords");

            migrationBuilder.DropColumn(
                name: "SubmissionId",
                table: "AnswerRecords");
        }
    }
}
