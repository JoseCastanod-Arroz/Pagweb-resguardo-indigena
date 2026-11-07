using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PagWebCabildo.Migrations
{
    /// <inheritdoc />
    public partial class SembrarDocumentosPrueba : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "CategoriasDocumento",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Categoría de prueba A" },
                    { 2, "Categoría de prueba B" }
                });

            migrationBuilder.InsertData(
                table: "Documentos",
                columns: new[] { "Id", "ArchivoRuta", "CategoriaDocumentoId", "Descripcion", "Titulo" },
                values: new object[,]
                {
                    { 1, "uploads/documentos/documento-prueba-1.txt", 1, "Este es un documento de PRUEBA. Reemplázalo por un archivo real.", "Documento de prueba 1" },
                    { 2, "uploads/documentos/documento-prueba-2.txt", 1, "Este es un documento de PRUEBA. Reemplázalo por un archivo real.", "Documento de prueba 2" },
                    { 3, "uploads/documentos/documento-prueba-3.txt", 2, "Este es un documento de PRUEBA. Reemplázalo por un archivo real.", "Documento de prueba 3" },
                    { 4, "uploads/documentos/documento-prueba-4.txt", 2, "Este es un documento de PRUEBA. Reemplázalo por un archivo real.", "Documento de prueba 4" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Documentos",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Documentos",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Documentos",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Documentos",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "CategoriasDocumento",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "CategoriasDocumento",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
