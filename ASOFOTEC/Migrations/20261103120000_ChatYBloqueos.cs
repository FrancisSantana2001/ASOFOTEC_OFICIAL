using System;
using ASOFOTEC.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASOFOTEC.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20261103120000_ChatYBloqueos")]
    public partial class ChatYBloqueos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bloqueos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BloqueadorId = table.Column<int>(type: "int", nullable: false),
                    BloqueadoId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bloqueos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bloqueos_usuarios_BloqueadoId",
                        column: x => x.BloqueadoId,
                        principalTable: "usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Bloqueos_usuarios_BloqueadorId",
                        column: x => x.BloqueadorId,
                        principalTable: "usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Conversaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioAId = table.Column<int>(type: "int", nullable: false),
                    UsuarioBId = table.Column<int>(type: "int", nullable: false),
                    Creada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UltimaActividad = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conversaciones_usuarios_UsuarioAId",
                        column: x => x.UsuarioAId,
                        principalTable: "usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Conversaciones_usuarios_UsuarioBId",
                        column: x => x.UsuarioBId,
                        principalTable: "usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Mensajes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConversacionId = table.Column<int>(type: "int", nullable: false),
                    RemitenteId = table.Column<int>(type: "int", nullable: false),
                    Texto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeidoEn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mensajes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mensajes_Conversaciones_ConversacionId",
                        column: x => x.ConversacionId,
                        principalTable: "Conversaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Mensajes_usuarios_RemitenteId",
                        column: x => x.RemitenteId,
                        principalTable: "usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bloqueos_BloqueadoId",
                table: "Bloqueos",
                column: "BloqueadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Bloqueos_BloqueadorId_BloqueadoId",
                table: "Bloqueos",
                columns: new[] { "BloqueadorId", "BloqueadoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conversaciones_UsuarioAId_UsuarioBId",
                table: "Conversaciones",
                columns: new[] { "UsuarioAId", "UsuarioBId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conversaciones_UsuarioBId",
                table: "Conversaciones",
                column: "UsuarioBId");

            migrationBuilder.CreateIndex(
                name: "IX_Mensajes_ConversacionId_Fecha",
                table: "Mensajes",
                columns: new[] { "ConversacionId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_Mensajes_RemitenteId",
                table: "Mensajes",
                column: "RemitenteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Bloqueos");
            migrationBuilder.DropTable(name: "Mensajes");
            migrationBuilder.DropTable(name: "Conversaciones");
        }
    }
}
