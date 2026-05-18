using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSoul.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSetVocabularyOverrideFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OverrideDescription",
                table: "SetVocabularies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OverrideExampleSentence",
                table: "SetVocabularies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OverrideMeaning",
                table: "SetVocabularies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OverridePronunciation",
                table: "SetVocabularies",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OverrideDescription",
                table: "SetVocabularies");

            migrationBuilder.DropColumn(
                name: "OverrideExampleSentence",
                table: "SetVocabularies");

            migrationBuilder.DropColumn(
                name: "OverrideMeaning",
                table: "SetVocabularies");

            migrationBuilder.DropColumn(
                name: "OverridePronunciation",
                table: "SetVocabularies");
        }
    }
}
