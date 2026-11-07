using System;
using ASOFOTEC.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASOFOTEC.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20261024150000_AgregarInteraccionesYMultimedia")]
    public partial class AgregarInteraccionesYMultimedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ----- Publicaciones: foto o video adjunto -----
            migrationBuilder.AddColumn<string>(
                name: "MediaTipo",
                table: "publicaciones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaUrl",
                table: "publicaciones",
                type: "nvarchar(max)",
                nullable: true);

            // ----- Comentarios: ahora pueden ser de una publicación y tener respuestas -----
            migrationBuilder.DropForeignKey(
                name: "FK_Comentarios_soluciones_SolucionId",
                table: "Comentarios");

            migrationBuilder.DropIndex(
                name: "IX_Comentarios_SolucionId",
                table: "Comentarios");

            migrationBuilder.AlterColumn<int>(
                name: "SolucionId",
                table: "Comentarios",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "ComentarioPadreId",
                table: "Comentarios",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PublicacionId",
                table: "Comentarios",
                type: "int",
                nullable: true);

            // ----- Likes de publicaciones y de comentarios -----
            migrationBuilder.CreateTable(
                name: "LikeComentarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Usuario_Id = table.Column<int>(type: "int", nullable: false),
                    Comentario_Id = table.Column<int>(type: "int", nullable: false),
                    FechaLike = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LikeComentarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LikeComentarios_Comentarios_Comentario_Id",
                        column: x => x.Comentario_Id,
                        principalTable: "Comentarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LikeComentarios_usuarios_Usuario_Id",
                        column: x => x.Usuario_Id,
                        principalTable: "usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LikePublicaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Usuario_Id = table.Column<int>(type: "int", nullable: false),
                    Publicacion_Id = table.Column<int>(type: "int", nullable: false),
                    FechaLike = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LikePublicaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LikePublicaciones_publicaciones_Publicacion_Id",
                        column: x => x.Publicacion_Id,
                        principalTable: "publicaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LikePublicaciones_usuarios_Usuario_Id",
                        column: x => x.Usuario_Id,
                        principalTable: "usuarios",
                        principalColumn: "Id");
                });

            // ----- Índices y claves foráneas -----
            migrationBuilder.CreateIndex(
                name: "IX_Comentarios_ComentarioPadreId",
                table: "Comentarios",
                column: "ComentarioPadreId");

            migrationBuilder.CreateIndex(
                name: "IX_Comentarios_PublicacionId",
                table: "Comentarios",
                column: "PublicacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Comentarios_SolucionId",
                table: "Comentarios",
                column: "SolucionId");

            migrationBuilder.CreateIndex(
                name: "IX_LikeComentarios_Comentario_Id",
                table: "LikeComentarios",
                column: "Comentario_Id");

            migrationBuilder.CreateIndex(
                name: "IX_LikeComentarios_Usuario_Id",
                table: "LikeComentarios",
                column: "Usuario_Id");

            migrationBuilder.CreateIndex(
                name: "IX_LikePublicaciones_Publicacion_Id",
                table: "LikePublicaciones",
                column: "Publicacion_Id");

            migrationBuilder.CreateIndex(
                name: "IX_LikePublicaciones_Usuario_Id",
                table: "LikePublicaciones",
                column: "Usuario_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comentarios_Comentarios_ComentarioPadreId",
                table: "Comentarios",
                column: "ComentarioPadreId",
                principalTable: "Comentarios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comentarios_publicaciones_PublicacionId",
                table: "Comentarios",
                column: "PublicacionId",
                principalTable: "publicaciones",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comentarios_soluciones_SolucionId",
                table: "Comentarios",
                column: "SolucionId",
                principalTable: "soluciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LikeComentarios");

            migrationBuilder.DropTable(
                name: "LikePublicaciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Comentarios_Comentarios_ComentarioPadreId",
                table: "Comentarios");

            migrationBuilder.DropForeignKey(
                name: "FK_Comentarios_publicaciones_PublicacionId",
                table: "Comentarios");

            migrationBuilder.DropForeignKey(
                name: "FK_Comentarios_soluciones_SolucionId",
                table: "Comentarios");

            // Los comentarios de publicaciones no pueden existir sin la columna PublicacionId
            migrationBuilder.Sql("DELETE FROM [Comentarios] WHERE [SolucionId] IS NULL");

            migrationBuilder.DropIndex(
                name: "IX_Comentarios_ComentarioPadreId",
                table: "Comentarios");

            migrationBuilder.DropIndex(
                name: "IX_Comentarios_PublicacionId",
                table: "Comentarios");

            migrationBuilder.DropIndex(
                name: "IX_Comentarios_SolucionId",
                table: "Comentarios");

            migrationBuilder.DropColumn(
                name: "ComentarioPadreId",
                table: "Comentarios");

            migrationBuilder.DropColumn(
                name: "PublicacionId",
                table: "Comentarios");

            migrationBuilder.DropColumn(
                name: "MediaTipo",
                table: "publicaciones");

            migrationBuilder.DropColumn(
                name: "MediaUrl",
                table: "publicaciones");

            migrationBuilder.AlterColumn<int>(
                name: "SolucionId",
                table: "Comentarios",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comentarios_SolucionId",
                table: "Comentarios",
                column: "SolucionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comentarios_soluciones_SolucionId",
                table: "Comentarios",
                column: "SolucionId",
                principalTable: "soluciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
