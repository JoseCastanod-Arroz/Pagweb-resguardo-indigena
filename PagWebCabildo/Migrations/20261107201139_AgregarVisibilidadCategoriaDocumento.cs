using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PagWebCabildo.Migrations
{
    /// <inheritdoc />
    public partial class AgregarVisibilidadCategoriaDocumento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Las categorías existentes eran todas públicas antes de esta función,
            // así que la columna se rellena con TRUE para no ocultar documentos que
            // ya estaban publicados. Las categorías nuevas toman su valor del modelo
            // (EsPublica = true por defecto) o del formulario de creación.
            migrationBuilder.AddColumn<bool>(
                name: "EsPublica",
                table: "CategoriasDocumento",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EsPublica",
                table: "CategoriasDocumento");
        }
    }
}
