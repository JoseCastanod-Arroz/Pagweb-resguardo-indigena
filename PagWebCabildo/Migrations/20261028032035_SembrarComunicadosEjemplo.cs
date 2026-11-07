using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PagWebCabildo.Migrations
{
    /// <inheritdoc />
    public partial class SembrarComunicadosEjemplo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Comunicados",
                columns: new[] { "Id", "Contenido", "Estado", "Fecha", "Titulo" },
                values: new object[,]
                {
                    { 1, "El cabildo convoca a todos los comuneros a la Asamblea General que se realizará en la casa del cabildo. En ella se presentarán los informes del año, se revisarán los proyectos en curso y se tomarán decisiones importantes para la comunidad. La participación de todas las familias es fundamental para el gobierno propio del resguardo.", 1, new DateTime(2026, 2, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Convocatoria a Asamblea General de la comunidad" },
                    { 2, "Invitamos a la comunidad a una jornada de limpieza de los caminos, las fuentes de agua y los espacios comunes del resguardo. Cuidar nuestro territorio es una responsabilidad compartida. Habrá refrigerio para quienes participen.", 1, new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Jornada de limpieza y cuidado del territorio" },
                    { 3, "Se abren las inscripciones para el taller dedicado a fortalecer la lengua, los saberes y las tradiciones del pueblo Pijao. Está dirigido a niños, jóvenes y adultos de la comunidad. Acércate a la casa del cabildo para registrar tu participación.", 1, new DateTime(2026, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Inscripciones abiertas para el taller de lengua y cultura Pijao" },
                    { 4, "El cabildo agradece a todas las familias que participaron en la minga comunitaria de siembra. Gracias al trabajo colectivo logramos sembrar la parcela comunitaria en un solo día. Este es el espíritu que mantiene vivo nuestro resguardo.", 1, new DateTime(2026, 3, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Agradecimiento por la minga de siembra" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Comunicados",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Comunicados",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Comunicados",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Comunicados",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
