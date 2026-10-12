using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASOFOTEC.Migrations
{
    /// <inheritdoc />
    public partial class Asofotec_Updated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "problemas",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "problemas");
        }
    }
}
