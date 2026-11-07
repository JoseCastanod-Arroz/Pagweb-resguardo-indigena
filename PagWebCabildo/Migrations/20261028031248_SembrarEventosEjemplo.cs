using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PagWebCabildo.Migrations
{
    /// <inheritdoc />
    public partial class SembrarEventosEjemplo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Eventos",
                columns: new[] { "Id", "Cancelado", "Descripcion", "Fecha", "Hora", "ImagenRuta", "Lugar", "Titulo" },
                values: new object[,]
                {
                    { 1, false, "Jornada de trabajo colectivo para la siembra de maíz, yuca y plátano en la parcela comunitaria. Compartimos semillas, saberes y un almuerzo preparado entre todas las familias. Trae tu herramienta y muchas ganas de compartir.", new DateOnly(2026, 3, 14), new TimeOnly(7, 0, 0), null, "Parcela comunitaria del resguardo", "Minga comunitaria de siembra" },
                    { 2, false, "Un encuentro tranquilo alrededor del fogón para escuchar a los mayores contar historias, mitos y memorias del pueblo Pijao. Habrá chicha y aromática para acompañar la conversación. Espacio abierto a niños, jóvenes y adultos.", new DateOnly(2026, 4, 25), new TimeOnly(18, 30, 0), null, "Casa del cabildo", "Noche de relatos de los mayores" },
                    { 3, false, "Celebración de nuestra cocina tradicional: tamales, insulso, bollos y bebidas de maíz. Música, danza y muestras de artesanía durante todo el día. Una jornada para compartir en familia y con los visitantes.", new DateOnly(2026, 6, 20), new TimeOnly(10, 0, 0), null, "Plaza central de Rincón Bodega", "Festival del maíz y la gastronomía propia" },
                    { 4, false, "Recorrido guiado por los linderos del resguardo para reconocer el territorio, los caminos y los sitios de importancia cultural. Actividad al aire libre, a ritmo pausado. Lleva agua, sombrero y calzado cómodo.", new DateOnly(2026, 8, 9), new TimeOnly(6, 30, 0), null, "Punto de encuentro: casa del cabildo", "Caminata por los linderos del territorio" },
                    { 5, false, "Tarde dedicada a aprender y compartir técnicas de tejido y elaboración de artesanías propias. Guiado por artesanas de la comunidad. Los materiales los pone el resguardo; solo trae tu creatividad.", new DateOnly(2026, 9, 12), new TimeOnly(14, 0, 0), null, "Casa del cabildo", "Taller de tejido y artesanía tradicional" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Eventos",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Eventos",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Eventos",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Eventos",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Eventos",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
