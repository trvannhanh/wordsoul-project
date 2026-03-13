using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSoul.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHintBalanceToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HintBalance",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HintBalance",
                table: "Users");
        }
    }
}
