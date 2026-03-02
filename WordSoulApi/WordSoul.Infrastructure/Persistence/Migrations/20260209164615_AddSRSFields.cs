using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSoul.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSRSFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EaseFactorAfter",
                table: "VocabularyReviewHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EaseFactorBefore",
                table: "VocabularyReviewHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IntervalAfter",
                table: "VocabularyReviewHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IntervalBefore",
                table: "VocabularyReviewHistories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextReviewAfter",
                table: "VocabularyReviewHistories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextReviewBefore",
                table: "VocabularyReviewHistories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CorrectCount",
                table: "UserVocabularyProgresses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FirstLearnedAt",
                table: "UserVocabularyProgresses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MasteredAt",
                table: "UserVocabularyProgresses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MemoryState",
                table: "UserVocabularyProgresses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "RetentionScore",
                table: "UserVocabularyProgresses",
                type: "decimal(5,4)",
                precision: 5,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "WrongCount",
                table: "UserVocabularyProgresses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HintCount",
                table: "AnswerRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "ResponseTimeSeconds",
                table: "AnswerRecords",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EaseFactorAfter",
                table: "VocabularyReviewHistories");

            migrationBuilder.DropColumn(
                name: "EaseFactorBefore",
                table: "VocabularyReviewHistories");

            migrationBuilder.DropColumn(
                name: "IntervalAfter",
                table: "VocabularyReviewHistories");

            migrationBuilder.DropColumn(
                name: "IntervalBefore",
                table: "VocabularyReviewHistories");

            migrationBuilder.DropColumn(
                name: "NextReviewAfter",
                table: "VocabularyReviewHistories");

            migrationBuilder.DropColumn(
                name: "NextReviewBefore",
                table: "VocabularyReviewHistories");

            migrationBuilder.DropColumn(
                name: "CorrectCount",
                table: "UserVocabularyProgresses");

            migrationBuilder.DropColumn(
                name: "FirstLearnedAt",
                table: "UserVocabularyProgresses");

            migrationBuilder.DropColumn(
                name: "MasteredAt",
                table: "UserVocabularyProgresses");

            migrationBuilder.DropColumn(
                name: "MemoryState",
                table: "UserVocabularyProgresses");

            migrationBuilder.DropColumn(
                name: "RetentionScore",
                table: "UserVocabularyProgresses");

            migrationBuilder.DropColumn(
                name: "WrongCount",
                table: "UserVocabularyProgresses");

            migrationBuilder.DropColumn(
                name: "HintCount",
                table: "AnswerRecords");

            migrationBuilder.DropColumn(
                name: "ResponseTimeSeconds",
                table: "AnswerRecords");
        }
    }
}
