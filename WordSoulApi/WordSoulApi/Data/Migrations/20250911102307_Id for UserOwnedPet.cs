using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSoulApi.Migrations
{
    /// <inheritdoc />
    public partial class IdforUserOwnedPet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserOwnedPets",
                table: "UserOwnedPets");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "UserOwnedPets",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserOwnedPets",
                table: "UserOwnedPets",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserOwnedPets_UserId",
                table: "UserOwnedPets",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserOwnedPets",
                table: "UserOwnedPets");

            migrationBuilder.DropIndex(
                name: "IX_UserOwnedPets_UserId",
                table: "UserOwnedPets");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserOwnedPets");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserOwnedPets",
                table: "UserOwnedPets",
                columns: new[] { "UserId", "PetId" });
        }
    }
}
