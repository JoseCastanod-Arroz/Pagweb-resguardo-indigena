using CsCheck;
using PagWebCabildo.Services;
using PagWebCabildo.Tests.Infraestructura;
using Xunit;

namespace PagWebCabildo.Tests.Services;

/// <summary>
/// Prueba basada en propiedades de la Property 9 del diseño para el registro de
/// mensajes del formulario de contacto.
///
/// Validates: Requirements 16.4
///
/// El sistema bajo prueba es
/// <see cref="MensajeService.RegistrarMensaje(string, string, string, string)"/>.
/// Según el Requisito 16.4, los cuatro campos del formulario (nombre, medio de
/// contacto, asunto y contenido) son obligatorios: si al menos uno está vacío o
/// compuesto únicamente por espacios, el envío se rechaza y no se registra ningún
/// mensaje.
///
/// Para cada iteración se genera un modelo en el que al menos uno de los cuatro
/// campos obligatorios queda en blanco (cadena vacía o solo espacios), mientras que
/// los demás pueden contener valores plausibles. Se crea una base SQLite en memoria
/// recién inicializada, se construye el servicio con una instancia real de
/// <see cref="ValidacionContactoService"/>, se invoca el registro y se verifica que:
/// <list type="bullet">
///   <item>el resultado indica fallo (<c>Exito == false</c>) y no devuelve mensaje
///   (<c>Mensaje == null</c>);</item>
///   <item>no se persistió ningún mensaje: un contexto nuevo observa la tabla de
///   mensajes vacía (recuento 0).</item>
/// </list>
/// </summary>
public class MensajeFormularioIncompletoPropertyTests
{
    private const int IteracionesMinimas = 100;

    // Feature: sitio-web-resguardo, Property 9: Un formulario con campos obligatorios vacíos se rechaza sin registrar mensaje
    [Fact]
    public void RegistrarMensaje_RechazaFormularioIncompletoSinRegistrar()
    {
        GenFormularioIncompleto()
            .Sample(datos =>
            {
                using var fixture = new SqliteEnMemoria();

                ResultadoRegistroMensaje resultado;
                using (var contextoServicio = fixture.CrearContexto())
                {
                    var servicio = new MensajeService(
                        contextoServicio,
                        new ValidacionContactoService());

                    resultado = servicio.RegistrarMensaje(
                            datos.Nombre,
                            datos.MedioContacto,
                            datos.Asunto,
                            datos.Contenido)
                        .GetAwaiter()
                        .GetResult();
                }

                // 1) El registro se rechaza y no devuelve mensaje.
                if (resultado.Exito || resultado.Mensaje is not null)
                {
                    return false;
                }

                // 2) No se persistió ningún mensaje: un contexto nuevo ve la tabla vacía.
                using var contextoLectura = fixture.CrearContexto();
                return contextoLectura.Mensajes.Count() == 0;
            }, iter: IteracionesMinimas);
    }

    /// <summary>
    /// Genera modelos de formulario en los que al menos uno de los cuatro campos
    /// obligatorios está en blanco (vacío o solo con espacios). Se elige al azar un
    /// subconjunto no vacío de campos a blanquear; los campos restantes reciben
    /// valores plausibles.
    /// </summary>
    private static Gen<(string Nombre, string MedioContacto, string Asunto, string Contenido)> GenFormularioIncompleto() =>
        // Máscara 1..15: cada bit indica qué campo se blanquea; al menos un bit activo
        // garantiza que siempre haya al menos un campo en blanco por iteración.
        Gen.Int[1, 15].SelectMany(mascara =>
            Gen.Select(
                GenCampo((mascara & 1) != 0, GenNombreValido()),
                GenCampo((mascara & 2) != 0, GenMedioValido()),
                GenCampo((mascara & 4) != 0, GenAsuntoValido()),
                GenCampo((mascara & 8) != 0, GenContenidoValido()),
                (nombre, medio, asunto, contenido) => (nombre, medio, asunto, contenido)));

    /// <summary>
    /// Devuelve un generador que produce un valor en blanco cuando
    /// <paramref name="enBlanco"/> es verdadero, o el valor plausible indicado en
    /// caso contrario.
    /// </summary>
    private static Gen<string> GenCampo(bool enBlanco, Gen<string> genValido) =>
        enBlanco ? GenEnBlanco() : genValido;

    /// <summary>Cadenas en blanco: vacías o compuestas solo por espacios en blanco.</summary>
    private static Gen<string> GenEnBlanco() =>
        Gen.OneOf(
            Gen.Const(string.Empty),
            Gen.Const(" "),
            Gen.Const("   "),
            Gen.Const("\t"),
            Gen.Const(" \t \n "));

    private static Gen<string> GenNombreValido() =>
        Gen.String[1, 40];

    private static Gen<string> GenMedioValido() =>
        Gen.OneOf(
            Gen.Const("persona@ejemplo.org"),
            Gen.Const("3001234567"),
            Gen.String[1, 20]);

    private static Gen<string> GenAsuntoValido() =>
        Gen.String[1, 40];

    private static Gen<string> GenContenidoValido() =>
        Gen.String[1, 80];
}
