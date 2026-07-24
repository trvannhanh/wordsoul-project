using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordSoul.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionVocabularyConcurrencyToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ConcurrencyToken",
                table: "SessionVocabularies",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE [SessionVocabularies] SET [ConcurrencyToken] = NEWID() " +
                "WHERE [ConcurrencyToken] IS NULL;");

            migrationBuilder.AlterColumn<Guid>(
                name: "ConcurrencyToken",
                table: "SessionVocabularies",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConcurrencyToken",
                table: "SessionVocabularies");
        }
    }
}
