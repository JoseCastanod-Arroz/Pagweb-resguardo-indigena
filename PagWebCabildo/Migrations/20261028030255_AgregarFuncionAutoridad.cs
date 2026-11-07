using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PagWebCabildo.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFuncionAutoridad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Funcion",
                table: "Autoridades",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Autoridades",
                keyColumn: "Id",
                keyValue: 1,
                column: "Funcion",
                value: "Representante legal del resguardo y primera autoridad gubernativa y administrativa. Preside las reuniones del cabildo y la comunidad, abre y ejecuta los proyectos junto con el tesorero, vigila el cumplimiento de las parcelas y los trabajos comunitarios, aprueba y ejecuta los proyectos en beneficio de la comunidad, hace cumplir las decisiones de la Asamblea y firma las sentencias y resoluciones del cabildo.");

            migrationBuilder.UpdateData(
                table: "Autoridades",
                keyColumn: "Id",
                keyValue: 2,
                column: "Funcion",
                value: "Asume las mismas funciones del gobernador titular cuando este se encuentra incapacitado, ausente o cuando lo autoriza.");

            migrationBuilder.UpdateData(
                table: "Autoridades",
                keyColumn: "Id",
                keyValue: 3,
                column: "Funcion",
                value: "Asiste a las reuniones del cabildo y elabora las respectivas actas, redacta los documentos que salen a nombre del resguardo, organiza el archivo, informa al gobernador sobre peticiones, denuncias y sugerencias, y firma junto al gobernador la correspondencia despachada.");

            migrationBuilder.UpdateData(
                table: "Autoridades",
                keyColumn: "Id",
                keyValue: 4,
                column: "Funcion",
                value: "Maneja los fondos de la comunidad llevando el registro de las cuentas, los informes y balances; consigna y retira los recursos del resguardo, realiza los contratos de compra y venta de bienes y servicios, y presenta informes del estado de la contabilidad permitiendo su acceso a la comunidad.");

            migrationBuilder.UpdateData(
                table: "Autoridades",
                keyColumn: "Id",
                keyValue: 5,
                column: "Funcion",
                value: "Vigila todas las acciones de los integrantes del resguardo, denuncia manejos o actuaciones irregulares de los cabildantes ante la Asamblea, y revisa los libros de contabilidad, la cuenta bancaria y las actas de la secretaría.");

            migrationBuilder.UpdateData(
                table: "Autoridades",
                keyColumn: "Id",
                keyValue: 6,
                column: "Funcion",
                value: "Acompaña al gobernador en las inspecciones oculares y en la solución de conflictos, atiende a las personas o entidades externas que visitan el resguardo, coordina los trabajos comunitarios y es el jefe de trabajo.");

            migrationBuilder.UpdateData(
                table: "Autoridades",
                keyColumn: "Id",
                keyValue: 7,
                column: "Funcion",
                value: "Cita, por orden previa y escrita del gobernador, a los comuneros y miembros del resguardo a las reuniones ordinarias y extraordinarias, y cumple las demás funciones que le asigne el gobernador conforme al reglamento interno.");

            migrationBuilder.UpdateData(
                table: "Autoridades",
                keyColumn: "Id",
                keyValue: 8,
                column: "Funcion",
                value: "Vela por el orden, la seguridad y la convivencia pacífica de la comunidad y de sus bienes, revisa periódicamente los linderos del resguardo, convoca a reuniones cuando se detectan irregularidades y adelanta el trámite de las quejas y denuncias instauradas por los miembros del resguardo.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Funcion",
                table: "Autoridades");
        }
    }
}
