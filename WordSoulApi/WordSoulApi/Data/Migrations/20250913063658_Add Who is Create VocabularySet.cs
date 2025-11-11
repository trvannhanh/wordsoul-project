using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSoulApi.Migrations
{
    /// <inheritdoc />
    public partial class AddWhoisCreateVocabularySet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedById",
                table: "VocabularySets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "VocabularySets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_VocabularySets_CreatedById",
                table: "VocabularySets",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_VocabularySets_Users_CreatedById",
                table: "VocabularySets",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VocabularySets_Users_CreatedById",
                table: "VocabularySets");

            migrationBuilder.DropIndex(
                name: "IX_VocabularySets_CreatedById",
                table: "VocabularySets");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "VocabularySets");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "VocabularySets");
        }
    }
}
