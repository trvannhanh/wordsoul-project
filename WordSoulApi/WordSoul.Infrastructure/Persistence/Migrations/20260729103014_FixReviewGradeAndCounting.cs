using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSoul.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixReviewGradeAndCounting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VocabularyReviewHistories_InitialRecallAnswerRecordId",
                table: "VocabularyReviewHistories");

            migrationBuilder.AddColumn<int>(
                name: "GradeReason",
                table: "VocabularyReviewHistories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GradingPolicyVersion",
                table: "VocabularyReviewHistories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InitialRecallCorrectCount",
                table: "UserVocabularyProgresses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InitialRecallCount",
                table: "UserVocabularyProgresses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LearningPracticeAttemptCount",
                table: "UserVocabularyProgresses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LearningPracticeSuccessCount",
                table: "UserVocabularyProgresses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RemediationAttemptCount",
                table: "UserVocabularyProgresses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RemediationSuccessCount",
                table: "UserVocabularyProgresses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InitialRecallGradeReason",
                table: "SessionVocabularies",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InitialRecallGradingPolicyVersion",
                table: "SessionVocabularies",
                type: "int",
                nullable: true);

            // Preserve the policy that produced outcomes before this migration.
            migrationBuilder.Sql(
                """
                UPDATE SessionVocabularies
                SET InitialRecallGradingPolicyVersion = 1,
                    InitialRecallGradeReason = 0
                WHERE InitialRecallAnswerRecordId IS NOT NULL;

                UPDATE VocabularyReviewHistories
                SET GradingPolicyVersion = 1,
                    GradeReason = 0
                WHERE InitialRecallAnswerRecordId IS NOT NULL;
                """);

            // Backfill only from versioned records whose phase was explicitly
            // persisted. Legacy counters remain untouched.
            migrationBuilder.Sql(
                """
                ;WITH InitialRecallMetrics AS
                (
                    SELECT
                        ls.UserId,
                        sv.VocabularyId,
                        COUNT(*) AS AttemptCount,
                        SUM(CASE WHEN sv.InitialRecallCorrect = 1 THEN 1 ELSE 0 END)
                            AS SuccessCount
                    FROM SessionVocabularies sv
                    INNER JOIN LearningSessions ls
                        ON ls.Id = sv.LearningSessionId
                    WHERE sv.InitialRecallAnswerRecordId IS NOT NULL
                    GROUP BY ls.UserId, sv.VocabularyId
                )
                UPDATE progress
                SET progress.InitialRecallCount = metrics.AttemptCount,
                    progress.InitialRecallCorrectCount = metrics.SuccessCount
                FROM UserVocabularyProgresses progress
                INNER JOIN InitialRecallMetrics metrics
                    ON metrics.UserId = progress.UserId
                   AND metrics.VocabularyId = progress.VocabularyId;

                ;WITH LearningMetrics AS
                (
                    SELECT
                        ls.UserId,
                        ar.VocabularyId,
                        COUNT(*) AS AttemptCount,
                        SUM(CASE WHEN ar.IsCorrect = 1 THEN 1 ELSE 0 END)
                            AS SuccessCount
                    FROM AnswerRecords ar
                    INNER JOIN LearningSessions ls
                        ON ls.Id = ar.LearningSessionId
                    WHERE ar.FlowVersion = 2
                      AND ls.Type = 0
                      AND ar.QuestionPhase IN (1, 2, 3)
                    GROUP BY ls.UserId, ar.VocabularyId
                )
                UPDATE progress
                SET progress.LearningPracticeAttemptCount = metrics.AttemptCount,
                    progress.LearningPracticeSuccessCount = metrics.SuccessCount
                FROM UserVocabularyProgresses progress
                INNER JOIN LearningMetrics metrics
                    ON metrics.UserId = progress.UserId
                   AND metrics.VocabularyId = progress.VocabularyId;

                ;WITH RemediationMetrics AS
                (
                    SELECT
                        ls.UserId,
                        ar.VocabularyId,
                        COUNT(*) AS AttemptCount,
                        SUM(CASE WHEN ar.IsCorrect = 1 THEN 1 ELSE 0 END)
                            AS SuccessCount
                    FROM AnswerRecords ar
                    INNER JOIN LearningSessions ls
                        ON ls.Id = ar.LearningSessionId
                    WHERE ar.FlowVersion = 2
                      AND ls.Type = 1
                      AND ar.QuestionPhase = 6
                    GROUP BY ls.UserId, ar.VocabularyId
                )
                UPDATE progress
                SET progress.RemediationAttemptCount = metrics.AttemptCount,
                    progress.RemediationSuccessCount = metrics.SuccessCount
                FROM UserVocabularyProgresses progress
                INNER JOIN RemediationMetrics metrics
                    ON metrics.UserId = progress.UserId
                   AND metrics.VocabularyId = progress.VocabularyId;

                UPDATE UserVocabularyProgresses
                SET RetentionScore =
                    CASE
                        WHEN
                            (
                                CAST(InitialRecallCorrectCount AS decimal(18, 6))
                                / InitialRecallCount * 100
                            )
                            + CASE
                                WHEN Repetition * 2 > 20 THEN 20
                                ELSE Repetition * 2
                              END > 100
                            THEN 100
                        ELSE
                            (
                                CAST(InitialRecallCorrectCount AS decimal(18, 6))
                                / InitialRecallCount * 100
                            )
                            + CASE
                                WHEN Repetition * 2 > 20 THEN 20
                                ELSE Repetition * 2
                              END
                    END
                WHERE InitialRecallCount > 0;
                """);

            // Keep all historical rows, but retain the audit link only on the
            // earliest row if inconsistent duplicate links already exist.
            migrationBuilder.Sql(
                """
                ;WITH RankedHistoryLinks AS
                (
                    SELECT
                        Id,
                        ROW_NUMBER() OVER
                        (
                            PARTITION BY InitialRecallAnswerRecordId
                            ORDER BY Id
                        ) AS RowNumber
                    FROM VocabularyReviewHistories
                    WHERE InitialRecallAnswerRecordId IS NOT NULL
                )
                UPDATE history
                SET InitialRecallAnswerRecordId = NULL
                FROM VocabularyReviewHistories history
                INNER JOIN RankedHistoryLinks ranked
                    ON ranked.Id = history.Id
                WHERE ranked.RowNumber > 1;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyReviewHistories_InitialRecallAnswerRecordId",
                table: "VocabularyReviewHistories",
                column: "InitialRecallAnswerRecordId",
                unique: true,
                filter: "[InitialRecallAnswerRecordId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VocabularyReviewHistories_InitialRecallAnswerRecordId",
                table: "VocabularyReviewHistories");

            migrationBuilder.DropColumn(
                name: "GradeReason",
                table: "VocabularyReviewHistories");

            migrationBuilder.DropColumn(
                name: "GradingPolicyVersion",
                table: "VocabularyReviewHistories");

            migrationBuilder.DropColumn(
                name: "InitialRecallCorrectCount",
                table: "UserVocabularyProgresses");

            migrationBuilder.DropColumn(
                name: "InitialRecallCount",
                table: "UserVocabularyProgresses");

            migrationBuilder.DropColumn(
                name: "LearningPracticeAttemptCount",
                table: "UserVocabularyProgresses");

            migrationBuilder.DropColumn(
                name: "LearningPracticeSuccessCount",
                table: "UserVocabularyProgresses");

            migrationBuilder.DropColumn(
                name: "RemediationAttemptCount",
                table: "UserVocabularyProgresses");

            migrationBuilder.DropColumn(
                name: "RemediationSuccessCount",
                table: "UserVocabularyProgresses");

            migrationBuilder.DropColumn(
                name: "InitialRecallGradeReason",
                table: "SessionVocabularies");

            migrationBuilder.DropColumn(
                name: "InitialRecallGradingPolicyVersion",
                table: "SessionVocabularies");

            migrationBuilder.CreateIndex(
                name: "IX_VocabularyReviewHistories_InitialRecallAnswerRecordId",
                table: "VocabularyReviewHistories",
                column: "InitialRecallAnswerRecordId");
        }
    }
}
