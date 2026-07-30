using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WordSoul.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureLearningAndSrsPolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SrsPolicyVersion",
                table: "VocabularyReviewHistories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLiveEditable",
                table: "SystemConfigurations",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<double>(
                name: "MaxValue",
                table: "SystemConfigurations",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MinValue",
                table: "SystemConfigurations",
                type: "float",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "AdminAppLogo",
                columns: new[] { "IsLiveEditable", "MaxValue", "MinValue" },
                values: new object[] { true, null, null });

            migrationBuilder.UpdateData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "AdminAppName",
                columns: new[] { "IsLiveEditable", "MaxValue", "MinValue" },
                values: new object[] { true, null, null });

            migrationBuilder.UpdateData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "AllowGoogleLogin",
                columns: new[] { "IsLiveEditable", "MaxValue", "MinValue" },
                values: new object[] { true, null, null });

            migrationBuilder.UpdateData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "AllowRegistration",
                columns: new[] { "IsLiveEditable", "MaxValue", "MinValue" },
                values: new object[] { true, null, null });

            migrationBuilder.UpdateData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "AppDisplayName",
                columns: new[] { "IsLiveEditable", "MaxValue", "MinValue" },
                values: new object[] { true, null, null });

            migrationBuilder.UpdateData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "CatchRateWrongPenalty",
                columns: new[] { "IsLiveEditable", "MaxValue", "MinValue" },
                values: new object[] { true, 1.0, 0.0 });

            migrationBuilder.UpdateData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "ContactEmail",
                columns: new[] { "IsLiveEditable", "MaxValue", "MinValue" },
                values: new object[] { true, null, null });

            migrationBuilder.UpdateData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "FacebookUrl",
                columns: new[] { "IsLiveEditable", "MaxValue", "MinValue" },
                values: new object[] { true, null, null });

            migrationBuilder.UpdateData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "FooterCopyright",
                columns: new[] { "IsLiveEditable", "MaxValue", "MinValue" },
                values: new object[] { true, null, null });

            migrationBuilder.UpdateData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "LogRetentionDays",
                columns: new[] { "IsLiveEditable", "MaxValue", "MinValue" },
                values: new object[] { true, null, null });

            migrationBuilder.UpdateData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "MaintenanceMode",
                columns: new[] { "IsLiveEditable", "MaxValue", "MinValue" },
                values: new object[] { true, null, null });

            migrationBuilder.UpdateData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "MaxGroupSize",
                columns: new[] { "IsLiveEditable", "MaxValue", "MinValue" },
                values: new object[] { true, null, null });

            migrationBuilder.UpdateData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "SrsInitialInterval1",
                columns: new[] { "Description", "IsLiveEditable", "MaxValue", "MinValue" },
                values: new object[] { "First interval in days for SM-2", true, 30.0, 0.0 });

            migrationBuilder.UpdateData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "SrsInitialInterval2",
                columns: new[] { "Description", "IsLiveEditable", "MaxValue", "MinValue" },
                values: new object[] { "Second interval in days for SM-2", true, 90.0, 1.0 });

            migrationBuilder.UpdateData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "SrsMinEf",
                columns: new[] { "IsLiveEditable", "MaxValue", "MinValue" },
                values: new object[] { true, 3.0, 1.0 });

            migrationBuilder.UpdateData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "WebAppFavicon",
                columns: new[] { "IsLiveEditable", "MaxValue", "MinValue" },
                values: new object[] { true, null, null });

            migrationBuilder.UpdateData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "WebAppLogo",
                columns: new[] { "IsLiveEditable", "MaxValue", "MinValue" },
                values: new object[] { true, null, null });

            migrationBuilder.UpdateData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "WebAppName",
                columns: new[] { "IsLiveEditable", "MaxValue", "MinValue" },
                values: new object[] { true, null, null });

            migrationBuilder.UpdateData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "WebAppSubtitle",
                columns: new[] { "IsLiveEditable", "MaxValue", "MinValue" },
                values: new object[] { true, null, null });

            migrationBuilder.UpdateData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "XpRewardNewSession",
                columns: new[] { "IsLiveEditable", "MaxValue", "MinValue" },
                values: new object[] { true, 10000.0, 0.0 });

            migrationBuilder.UpdateData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "XpRewardReviewSession",
                columns: new[] { "IsLiveEditable", "MaxValue", "MinValue" },
                values: new object[] { true, 10000.0, 0.0 });

            // Preserve values that may already have been created manually in
            // production while adding typed metadata required by the Admin UI.
            migrationBuilder.Sql(
                """
                MERGE SystemConfigurations AS target
                USING
                (
                    VALUES
                    ('ReviewBaseAP', '3', 'Integer', 'GAME_BALANCE',
                     'Base AP rewarded for completing a review session',
                     CAST(0 AS float), CAST(1000 AS float), CAST(1 AS bit)),
                    ('SrsDefaultEf', '2.5', 'Float', 'SRS',
                     'Initial Ease Factor assigned to new vocabulary progress',
                     CAST(1.3 AS float), CAST(4 AS float), CAST(1 AS bit)),
                    ('SrsMasteredIntervalDays', '21', 'Integer', 'SRS',
                     'Minimum interval in days for a vocabulary to be considered mastered',
                     CAST(7 AS float), CAST(365 AS float), CAST(1 AS bit)),
                    ('SrsMaxEf', '4.0', 'Float', 'SRS',
                     'Maximum Ease Factor for SM-2 Algorithm',
                     CAST(1.3 AS float), CAST(5 AS float), CAST(1 AS bit)),
                    ('SrsPolicyVersion', '1', 'Integer', 'SRS',
                     'Automatically incremented whenever SRS algorithm settings change',
                     CAST(1 AS float), CAST(NULL AS float), CAST(0 AS bit)),
                    ('SrsRetentionBonusMax', '20', 'Float', 'SRS',
                     'Maximum retention score bonus from repetitions',
                     CAST(0 AS float), CAST(100 AS float), CAST(1 AS bit)),
                    ('SrsRetentionBonusPerRepetition', '2', 'Float', 'SRS',
                     'Retention score bonus per successful repetition',
                     CAST(0 AS float), CAST(10 AS float), CAST(1 AS bit)),
                    ('WordsPerSession', '5', 'Integer', 'LEARNING',
                     'Number of vocabulary items included in each learning or review session',
                     CAST(1 AS float), CAST(30 AS float), CAST(1 AS bit))
                )
                AS source
                (
                    [Key], [Value], DataType, Category, Description,
                    MinValue, MaxValue, IsLiveEditable
                )
                ON target.[Key] = source.[Key]
                WHEN MATCHED THEN
                    UPDATE SET
                        target.DataType = source.DataType,
                        target.Category = source.Category,
                        target.Description = source.Description,
                        target.MinValue = source.MinValue,
                        target.MaxValue = source.MaxValue,
                        target.IsLiveEditable = source.IsLiveEditable
                WHEN NOT MATCHED THEN
                    INSERT
                    (
                        [Key], [Value], DataType, Category, Description,
                        MinValue, MaxValue, IsLiveEditable,
                        LastUpdatedAt, LastUpdatedBy
                    )
                    VALUES
                    (
                        source.[Key], source.[Value], source.DataType,
                        source.Category, source.Description, source.MinValue,
                        source.MaxValue, source.IsLiveEditable,
                        '2024-01-01T00:00:00', 'System'
                    );

                UPDATE VocabularyReviewHistories
                SET SrsPolicyVersion = 1
                WHERE SrsPolicyVersion IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SrsPolicyVersion",
                table: "VocabularyReviewHistories");

            migrationBuilder.DropColumn(
                name: "IsLiveEditable",
                table: "SystemConfigurations");

            migrationBuilder.DropColumn(
                name: "MaxValue",
                table: "SystemConfigurations");

            migrationBuilder.DropColumn(
                name: "MinValue",
                table: "SystemConfigurations");

            migrationBuilder.UpdateData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "SrsInitialInterval1",
                column: "Description",
                value: "First interval (days) for SM-2");

            migrationBuilder.UpdateData(
                table: "SystemConfigurations",
                keyColumn: "Key",
                keyValue: "SrsInitialInterval2",
                column: "Description",
                value: "Second interval (days) for SM-2");
        }
    }
}
