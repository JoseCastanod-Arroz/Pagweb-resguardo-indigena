using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PagWebCabildo.Migrations
{
    /// <inheritdoc />
    public partial class SeedInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Administradores",
                columns: new[] { "Id", "Activo", "HashContraseña", "Nombre", "NombreUsuario", "Rol" },
                values: new object[] { 1, true, "AQAAAAIAAYagAAAAEMIqadTtIKxWfSEBUsfxIq+G1rs4BdZjHbPLGpQvm8oCW4EpdD3HpC7z6/a6oiFBEQ==", "Administrador General", "admin", 0 });

            migrationBuilder.InsertData(
                table: "InformacionesContacto",
                columns: new[] { "Id", "CorreoElectronico", "Direccion", "OtrosDatos", "Telefono" },
                values: new object[] { 1, "contacto@resguardo.example", "Dirección del resguardo (pendiente de configurar)", "Otros datos de contacto (pendiente de configurar)", "0000000" });

            migrationBuilder.InsertData(
                table: "Presentaciones",
                columns: new[] { "Id", "Texto" },
                values: new object[] { 1, "Texto de presentación del Resguardo Indígena Rincón Bodega. Edite este contenido desde el Panel de Administración." });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Administradores",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "InformacionesContacto",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Presentaciones",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
