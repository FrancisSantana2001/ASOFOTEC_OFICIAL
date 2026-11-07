using System;
using ASOFOTEC.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASOFOTEC.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20261026160000_AgregarRutaDelProblema")]
    public partial class AgregarRutaDelProblema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ----- Problemas: causas y consecuencias (paso 2) -----
            migrationBuilder.AddColumn<string>(
                name: "Causas",
                table: "problemas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Consecuencias",
                table: "problemas",
                type: "nvarchar(max)",
                nullable: true);

            // ----- Soluciones: comparación, selección y resultados (pasos 4, 5 y 7) -----
            migrationBuilder.AddColumn<string>(
                name: "Costo",
                table: "soluciones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaResultados",
                table: "soluciones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Impacto",
                table: "soluciones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Resultados",
                table: "soluciones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Seleccionada",
                table: "soluciones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TiempoEstimado",
                table: "soluciones",
                type: "nvarchar(max)",
                nullable: true);

            // ----- Equipo (paso 5) y avances (paso 6) -----
            migrationBuilder.CreateTable(
                name: "Avances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolucionId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Contenido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Porcentaje = table.Column<int>(type: "int", nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Avances_soluciones_SolucionId",
                        column: x => x.SolucionId,
                        principalTable: "soluciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Avances_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Colaboradores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolucionId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    FechaUnion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colaboradores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Colaboradores_soluciones_SolucionId",
                        column: x => x.SolucionId,
                        principalTable: "soluciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Colaboradores_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Avances_SolucionId",
                table: "Avances",
                column: "SolucionId");

            migrationBuilder.CreateIndex(
                name: "IX_Avances_UsuarioId",
                table: "Avances",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Colaboradores_SolucionId_UsuarioId",
                table: "Colaboradores",
                columns: new[] { "SolucionId", "UsuarioId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Colaboradores_UsuarioId",
                table: "Colaboradores",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Avances");

            migrationBuilder.DropTable(
                name: "Colaboradores");

            migrationBuilder.DropColumn(name: "Causas", table: "problemas");
            migrationBuilder.DropColumn(name: "Consecuencias", table: "problemas");

            migrationBuilder.DropColumn(name: "Costo", table: "soluciones");
            migrationBuilder.DropColumn(name: "FechaResultados", table: "soluciones");
            migrationBuilder.DropColumn(name: "Impacto", table: "soluciones");
            migrationBuilder.DropColumn(name: "Resultados", table: "soluciones");
            migrationBuilder.DropColumn(name: "Seleccionada", table: "soluciones");
            migrationBuilder.DropColumn(name: "TiempoEstimado", table: "soluciones");
        }
    }
}
