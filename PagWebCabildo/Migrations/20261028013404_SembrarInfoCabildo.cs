using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PagWebCabildo.Migrations
{
    /// <inheritdoc />
    public partial class SembrarInfoCabildo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Autoridades",
                columns: new[] { "Id", "Cargo", "Nombre", "Orden", "TipoAutoridad" },
                values: new object[,]
                {
                    { 1, "Gobernador", "Hugo Loaiza", 1, "Cabildo" },
                    { 2, "Gobernadora Suplente", "Graciela Capera Cumaco", 2, "Cabildo" },
                    { 3, "Secretario(a)", "Por definir", 3, "Cabildo" },
                    { 4, "Tesorero(a)", "Por definir", 4, "Cabildo" },
                    { 5, "Fiscal", "Por definir", 5, "Cabildo" },
                    { 6, "Alcalde Mayor", "Por definir", 6, "Cabildo" },
                    { 7, "Comisario(a)", "Por definir", 7, "Cabildo" },
                    { 8, "Alguacil", "Por definir", 8, "Cabildo" }
                });

            migrationBuilder.UpdateData(
                table: "InformacionesContacto",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CorreoElectronico", "Direccion", "OtrosDatos" },
                values: new object[] { "contacto@resguardorinconbodega.org", "Resguardo Indígena Rincón Bodega, municipio de Natagaima, Tolima (Colombia)", "Comunidad Pijao. NIT. 900087502-4. Reconocido por Resolución N.° 0055 del 18 de diciembre de 2000 (INCORA). Nota: actualice el teléfono y el correo oficiales desde el Panel de Administración." });

            migrationBuilder.UpdateData(
                table: "Presentaciones",
                keyColumn: "Id",
                keyValue: 1,
                column: "Texto",
                value: "El Resguardo Indígena Rincón Bodega es una comunidad del pueblo Pijao, ubicada en el municipio de Natagaima, departamento del Tolima (Colombia). Fue reconocido mediante la Resolución N.° 0055 del 18 de diciembre de 2000 del INCORA (NIT. 900087502-4).\n\nNuestra comunidad y nuestro cabildo ejercen su autonomía al amparo de la Ley 89 de 1890 y de los artículos 246 y 330 de la Constitución Política de 1991, ratificados por la Sentencia C-139 de 1996 de la Honorable Corte Constitucional, como un sistema propio que regula nuestra tradición, usos, costumbres y cultura.\n\nEl Resguardo trabaja por defender los derechos e intereses de la población indígena Pijao del Tolima, promover el conocimiento y la investigación de nuestra realidad conforme al uso, la costumbre y la cultura de la etnia, fortalecer la unión con otras organizaciones populares en busca del bien común, y recuperar y fortalecer nuestros usos, costumbres e identidad cultural.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Autoridades",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Autoridades",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Autoridades",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Autoridades",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Autoridades",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Autoridades",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Autoridades",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Autoridades",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.UpdateData(
                table: "InformacionesContacto",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CorreoElectronico", "Direccion", "OtrosDatos" },
                values: new object[] { "contacto@resguardo.example", "Dirección del resguardo (pendiente de configurar)", "Otros datos de contacto (pendiente de configurar)" });

            migrationBuilder.UpdateData(
                table: "Presentaciones",
                keyColumn: "Id",
                keyValue: 1,
                column: "Texto",
                value: "Texto de presentación del Resguardo Indígena Rincón Bodega. Edite este contenido desde el Panel de Administración.");
        }
    }
}
