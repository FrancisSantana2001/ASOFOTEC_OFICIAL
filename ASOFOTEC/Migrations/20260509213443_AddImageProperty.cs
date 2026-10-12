using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASOFOTEC.Migrations
{
    /// <inheritdoc />
    public partial class AddImageProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImgeUrl",
                table: "problemas",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImgeUrl",
                table: "problemas");
        }
    }
}
