using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PagWebCabildo.Migrations
{
    /// <inheritdoc />
    public partial class SembrarGaleriaEjemplo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TemasGaleria",
                columns: new[] { "Id", "Descripcion", "Nombre", "PortadaRuta" },
                values: new object[] { 1, "Paisajes, vida cotidiana y encuentros de la comunidad del Resguardo Indígena Rincón Bodega.", "Territorio y comunidad", "uploads/galeria/portadas/muestra-portada.jpeg" });

            migrationBuilder.InsertData(
                table: "ElementosGaleria",
                columns: new[] { "Id", "ArchivoRuta", "Descripcion", "TemaGaleriaId", "Tipo" },
                values: new object[,]
                {
                    { 1, "uploads/galeria/elementos/muestra-1.jpeg", "Paisaje del territorio del resguardo.", 1, 0 },
                    { 2, "uploads/galeria/elementos/muestra-2.jpeg", "Vida cotidiana en la comunidad.", 1, 0 },
                    { 3, "uploads/galeria/elementos/muestra-3.png", "Tradición y cultura Pijao.", 1, 0 },
                    { 4, "uploads/galeria/elementos/muestra-4.jpeg", "Naturaleza alrededor de Rincón Bodega.", 1, 0 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ElementosGaleria",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ElementosGaleria",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ElementosGaleria",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ElementosGaleria",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "TemasGaleria",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
