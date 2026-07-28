using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSoul.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVersionedQuestionFlows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "InitialRecallCorrect",
                table: "SessionVocabularies",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InitialRecallGrade",
                table: "SessionVocabularies",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FlowVersion",
                table: "LearningSessions",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InitialRecallCorrect",
                table: "SessionVocabularies");

            migrationBuilder.DropColumn(
                name: "InitialRecallGrade",
                table: "SessionVocabularies");

            migrationBuilder.DropColumn(
                name: "FlowVersion",
                table: "LearningSessions");
        }
    }
}
