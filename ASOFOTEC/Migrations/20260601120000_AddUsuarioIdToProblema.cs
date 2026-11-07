using ASOFOTEC.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASOFOTEC.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260601120000_AddUsuarioIdToProblema")]
    public partial class AddUsuarioIdToProblema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "problemas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_problemas_UsuarioId",
                table: "problemas",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_problemas_usuarios_UsuarioId",
                table: "problemas",
                column: "UsuarioId",
                principalTable: "usuarios",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_problemas_usuarios_UsuarioId",
                table: "problemas");

            migrationBuilder.DropIndex(
                name: "IX_problemas_UsuarioId",
                table: "problemas");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "problemas");
        }
    }
}
