using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASOFOTEC.Migrations
{
    /// <inheritdoc />
    public partial class AddComentariosYLikes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Comentarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Contenido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaComentario = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    SolucionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comentarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comentarios_soluciones_SolucionId",
                        column: x => x.SolucionId,
                        principalTable: "soluciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comentarios_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LikeSoluciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Usuario_Id = table.Column<int>(type: "int", nullable: false),
                    Solucion_Id = table.Column<int>(type: "int", nullable: false),
                    FechaLike = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LikeSoluciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LikeSoluciones_soluciones_Solucion_Id",
                        column: x => x.Solucion_Id,
                        principalTable: "soluciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LikeSoluciones_usuarios_Usuario_Id",
                        column: x => x.Usuario_Id,
                        principalTable: "usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comentarios_SolucionId",
                table: "Comentarios",
                column: "SolucionId");

            migrationBuilder.CreateIndex(
                name: "IX_Comentarios_UsuarioId",
                table: "Comentarios",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_LikeSoluciones_Solucion_Id",
                table: "LikeSoluciones",
                column: "Solucion_Id");

            migrationBuilder.CreateIndex(
                name: "IX_LikeSoluciones_Usuario_Id",
                table: "LikeSoluciones",
                column: "Usuario_Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comentarios");

            migrationBuilder.DropTable(
                name: "LikeSoluciones");
        }
    }
}
