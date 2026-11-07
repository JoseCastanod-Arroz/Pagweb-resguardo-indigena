using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PagWebCabildo.Migrations
{
    /// <inheritdoc />
    public partial class AgregarActasRegistroActividadFinanciero : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EsFinanciero",
                table: "CategoriasDocumento",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Actas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Consecutivo = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaAsamblea = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    Titulo = table.Column<string>(type: "TEXT", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: false),
                    ArchivoRuta = table.Column<string>(type: "TEXT", nullable: false),
                    RegistradoPorId = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Actas_Administradores_RegistradoPorId",
                        column: x => x.RegistradoPorId,
                        principalTable: "Administradores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosActividad",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AdministradorId = table.Column<int>(type: "INTEGER", nullable: false),
                    Accion = table.Column<string>(type: "TEXT", nullable: false),
                    Modulo = table.Column<string>(type: "TEXT", nullable: false),
                    Detalle = table.Column<string>(type: "TEXT", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosActividad", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "CategoriasDocumento",
                keyColumn: "Id",
                keyValue: 1,
                column: "EsFinanciero",
                value: false);

            migrationBuilder.UpdateData(
                table: "CategoriasDocumento",
                keyColumn: "Id",
                keyValue: 2,
                column: "EsFinanciero",
                value: false);

            migrationBuilder.CreateIndex(
                name: "IX_Actas_Consecutivo",
                table: "Actas",
                column: "Consecutivo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Actas_RegistradoPorId",
                table: "Actas",
                column: "RegistradoPorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Actas");

            migrationBuilder.DropTable(
                name: "RegistrosActividad");

            migrationBuilder.DropColumn(
                name: "EsFinanciero",
                table: "CategoriasDocumento");
        }
    }
}
