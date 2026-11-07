using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PagWebCabildo.Migrations
{
    /// <inheritdoc />
    public partial class RedactarFuncionesAutoridad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Autoridades",
                keyColumn: "Id",
                keyValue: 1,
                column: "Funcion",
                value: "El Gobernador es la máxima autoridad del cabildo y el representante legal del resguardo ante la comunidad, el Estado y otras organizaciones. Dirige la vida institucional del resguardo: preside las reuniones del cabildo y las asambleas de la comunidad, orienta la ejecución de los proyectos y, junto con el Tesorero, administra los recursos comunes. Vela por el uso adecuado de las parcelas y por el cumplimiento de los trabajos comunitarios, aprueba las iniciativas que benefician a la comunidad y se asegura de que las decisiones tomadas en asamblea se lleven a la práctica. También firma las resoluciones, sentencias y documentos oficiales que el cabildo expide en ejercicio de la justicia propia.");

            migrationBuilder.UpdateData(
                table: "Autoridades",
                keyColumn: "Id",
                keyValue: 2,
                column: "Funcion",
                value: "La Gobernadora Suplente acompaña al Gobernador en el ejercicio de su cargo y garantiza la continuidad del gobierno propio. Cuando el Gobernador se encuentra ausente, incapacitado o así lo autoriza, asume plenamente sus mismas funciones y responsabilidades, de modo que el resguardo nunca quede sin dirección ni representación.");

            migrationBuilder.UpdateData(
                table: "Autoridades",
                keyColumn: "Id",
                keyValue: 3,
                column: "Funcion",
                value: "El Secretario es responsable de la memoria escrita del resguardo. Asiste a las reuniones del cabildo y redacta las actas que dejan constancia de lo acordado, prepara los documentos y la correspondencia que se emiten a nombre de la comunidad y mantiene organizado el archivo. Además, informa al Gobernador sobre las peticiones, denuncias y sugerencias que llegan de los comuneros, y firma, junto con él, la documentación oficial. Su labor asegura que las decisiones queden registradas y puedan consultarse.");

            migrationBuilder.UpdateData(
                table: "Autoridades",
                keyColumn: "Id",
                keyValue: 4,
                column: "Funcion",
                value: "El Tesorero administra los recursos económicos del resguardo con transparencia. Lleva el registro ordenado de los ingresos y gastos, elabora los informes y balances, y gestiona el dinero de la comunidad: consigna y retira fondos y realiza los contratos de compra y venta de bienes y servicios necesarios. Rinde cuentas de manera periódica y mantiene la contabilidad abierta a la revisión de la comunidad, de forma que todos puedan conocer en qué se invierten los recursos comunes.");

            migrationBuilder.UpdateData(
                table: "Autoridades",
                keyColumn: "Id",
                keyValue: 5,
                column: "Funcion",
                value: "El Fiscal cumple una función de control y vigilancia dentro del resguardo. Supervisa que las acciones de los integrantes del cabildo y de la comunidad se ajusten al reglamento interno y a la ley propia, y revisa los libros de contabilidad, la cuenta bancaria y las actas de la Secretaría. Cuando detecta manejos indebidos o actuaciones irregulares, los denuncia ante la Asamblea para que se tomen las medidas correspondientes. Su papel protege el buen uso de los recursos y la confianza de la comunidad.");

            migrationBuilder.UpdateData(
                table: "Autoridades",
                keyColumn: "Id",
                keyValue: 6,
                column: "Funcion",
                value: "El Alcalde Mayor apoya directamente al Gobernador en las tareas de campo y en la convivencia diaria del resguardo. Lo acompaña en las inspecciones del territorio y en la solución de conflictos, atiende a las personas y entidades externas que visitan la comunidad, y coordina y encabeza los trabajos comunitarios. Como jefe de trabajo, organiza las labores colectivas y procura que se realicen de manera ordenada y en beneficio de todos.");

            migrationBuilder.UpdateData(
                table: "Autoridades",
                keyColumn: "Id",
                keyValue: 7,
                column: "Funcion",
                value: "El Comisario se encarga de convocar y comunicar a la comunidad. Por indicación previa y escrita del Gobernador, cita a los comuneros y a los miembros del resguardo a las reuniones ordinarias y extraordinarias, y se asegura de que la información llegue a tiempo. Cumple, además, las demás tareas de coordinación que el Gobernador le asigne conforme al reglamento interno, facilitando la participación de todos en la vida del resguardo.");

            migrationBuilder.UpdateData(
                table: "Autoridades",
                keyColumn: "Id",
                keyValue: 8,
                column: "Funcion",
                value: "El Alguacil vela por el orden, la seguridad y la convivencia pacífica de la comunidad y por el cuidado de sus bienes. Recorre y revisa periódicamente los linderos del resguardo para proteger el territorio, convoca a reuniones cuando advierte irregularidades y da trámite a las quejas y denuncias que presentan los miembros del resguardo. Su labor contribuye a mantener la armonía interna y el respeto por las normas propias.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
