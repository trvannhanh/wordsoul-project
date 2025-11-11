using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSoulApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAPointforUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AP",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "VocabularySetId",
                table: "LearningSessions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AP",
                table: "Users");

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
