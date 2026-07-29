using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSoul.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CaptureInitialRecallOutcome : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InitialRecallAnswerRecordId",
                table: "VocabularyReviewHistories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InitialRecallAnswerRecordId",
                table: "SessionVocabularies",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InitialRecallAt",
                table: "SessionVocabularies",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FlowVersion",
                table: "AnswerRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuestionPhase",
                table: "AnswerRecords",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StageIndexBefore",
                table: "AnswerRecords",
                type: "int",
                nullable: true);

            // FlowVersion is safe to backfill for every historical answer because
            // the owning learning session is immutable.
            migrationBuilder.Sql(
                """
                UPDATE ar
                SET ar.FlowVersion = ls.FlowVersion
                FROM AnswerRecords ar
                INNER JOIN LearningSessions ls ON ls.Id = ar.LearningSessionId;
                """);

            // Current Review sessions already persisted the locked grade/correctness.
            // Link those snapshots to their chronologically first answer. Other
            // phase/stage values stay null because they cannot be inferred safely.
            migrationBuilder.Sql(
                """
                ;WITH RankedReviewAnswers AS
                (
                    SELECT
                        ar.Id,
                        ar.LearningSessionId,
                        ar.VocabularyId,
                        ar.CreatedAt,
                        ROW_NUMBER() OVER
                        (
                            PARTITION BY ar.LearningSessionId, ar.VocabularyId
                            ORDER BY ar.CreatedAt, ar.Id
                        ) AS RowNumber
                    FROM AnswerRecords ar
                    INNER JOIN LearningSessions ls
                        ON ls.Id = ar.LearningSessionId
                    WHERE ls.Type = 1
                      AND ls.FlowVersion = 2
                )
                UPDATE ar
                SET ar.QuestionPhase = 4,
                    ar.StageIndexBefore = 0
                FROM AnswerRecords ar
                INNER JOIN RankedReviewAnswers ranked ON ranked.Id = ar.Id
                WHERE ranked.RowNumber = 1;

                ;WITH RankedReviewAnswers AS
                (
                    SELECT
                        ar.Id,
                        ar.LearningSessionId,
                        ar.VocabularyId,
                        ar.CreatedAt,
                        ROW_NUMBER() OVER
                        (
                            PARTITION BY ar.LearningSessionId, ar.VocabularyId
                            ORDER BY ar.CreatedAt, ar.Id
                        ) AS RowNumber
                    FROM AnswerRecords ar
                    INNER JOIN LearningSessions ls
                        ON ls.Id = ar.LearningSessionId
                    WHERE ls.Type = 1
                      AND ls.FlowVersion = 2
                )
                UPDATE sv
                SET sv.InitialRecallAnswerRecordId = ranked.Id,
                    sv.InitialRecallAt = ranked.CreatedAt
                FROM SessionVocabularies sv
                INNER JOIN RankedReviewAnswers ranked
                    ON ranked.LearningSessionId = sv.LearningSessionId
                   AND ranked.VocabularyId = sv.VocabularyId
                   AND ranked.RowNumber = 1
                WHERE sv.InitialRecallGrade IS NOT NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyReviewHistories_InitialRecallAnswerRecordId",
                table: "VocabularyReviewHistories",
                column: "InitialRecallAnswerRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionVocabularies_InitialRecallAnswerRecordId",
                table: "SessionVocabularies",
                column: "InitialRecallAnswerRecordId",
                unique: true,
                filter: "[InitialRecallAnswerRecordId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_SessionVocabularies_AnswerRecords_InitialRecallAnswerRecordId",
                table: "SessionVocabularies",
                column: "InitialRecallAnswerRecordId",
                principalTable: "AnswerRecords",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VocabularyReviewHistories_AnswerRecords_InitialRecallAnswerRecordId",
                table: "VocabularyReviewHistories",
                column: "InitialRecallAnswerRecordId",
                principalTable: "AnswerRecords",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SessionVocabularies_AnswerRecords_InitialRecallAnswerRecordId",
                table: "SessionVocabularies");

            migrationBuilder.DropForeignKey(
                name: "FK_VocabularyReviewHistories_AnswerRecords_InitialRecallAnswerRecordId",
                table: "VocabularyReviewHistories");

            migrationBuilder.DropIndex(
                name: "IX_VocabularyReviewHistories_InitialRecallAnswerRecordId",
                table: "VocabularyReviewHistories");

            migrationBuilder.DropIndex(
                name: "IX_SessionVocabularies_InitialRecallAnswerRecordId",
                table: "SessionVocabularies");

            migrationBuilder.DropColumn(
                name: "InitialRecallAnswerRecordId",
                table: "VocabularyReviewHistories");

            migrationBuilder.DropColumn(
                name: "InitialRecallAnswerRecordId",
                table: "SessionVocabularies");

            migrationBuilder.DropColumn(
                name: "InitialRecallAt",
                table: "SessionVocabularies");

            migrationBuilder.DropColumn(
                name: "FlowVersion",
                table: "AnswerRecords");

            migrationBuilder.DropColumn(
                name: "QuestionPhase",
                table: "AnswerRecords");

            migrationBuilder.DropColumn(
                name: "StageIndexBefore",
                table: "AnswerRecords");
        }
    }
}
