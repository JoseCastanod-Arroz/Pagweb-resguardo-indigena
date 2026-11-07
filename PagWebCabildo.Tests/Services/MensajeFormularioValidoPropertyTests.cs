using System;
using System.Linq;
using CsCheck;
using PagWebCabildo.Services;
using PagWebCabildo.Tests.Infraestructura;
using Xunit;

namespace PagWebCabildo.Tests.Services;

/// <summary>
/// Prueba basada en propiedades de la Property 10 del diseño para el registro de un
/// formulario de contacto válido.
///
/// Validates: Requirements 16.3, 17.1
///
/// El sistema bajo prueba es
/// <see cref="MensajeService.RegistrarMensaje(string, string, string, string)"/>.
/// Según los Requisitos 16.3 y 17.1, cuando el formulario es válido (nombre 1–100,
/// asunto 1–150, contenido 1–2000, los tres no vacíos tras recortar, y un medio de
/// contacto que es un correo válido o un teléfono de 7 a 15 dígitos), el servicio
/// registra un mensaje recuperable cuyos datos coinciden con lo ingresado (ya
/// recortado), con <see cref="PagWebCabildo.Models.Entidades.Mensaje.FechaRecepcion"/>
/// establecida y <see cref="PagWebCabildo.Models.Entidades.Mensaje.Atendido"/> en
/// <c>false</c>.
///
/// Para cada iteración se genera un modelo válido dentro de los límites de longitud
/// y sin espacios al inicio o al final (para que la comparación tras el recorte sea
/// exacta); se crea una base SQLite en memoria recién inicializada, se construye el
/// servicio con una instancia real de <see cref="ValidacionContactoService"/>, se
/// invoca <c>RegistrarMensaje</c> y se verifica que el registro tuvo éxito y que el
/// mensaje es recuperable desde un contexto independiente por su Id con los datos
/// esperados.
/// </summary>
public class MensajeFormularioValidoPropertyTests
{
    private const int IteracionesMinimas = 100;

    // Feature: sitio-web-resguardo, Property 10: Un formulario válido registra el mensaje de forma recuperable
    [Fact]
    public void RegistrarMensaje_FormularioValidoRegistraMensajeRecuperable()
    {
        GenModeloValido()
            .Sample(datos =>
            {
                using var fixture = new SqliteEnMemoria();

                using var contextoServicio = fixture.CrearContexto();
                var servicio = new MensajeService(contextoServicio, new ValidacionContactoService());

                var resultado = servicio
                    .RegistrarMensaje(datos.Nombre, datos.MedioContacto, datos.Asunto, datos.Contenido)
                    .GetAwaiter()
                    .GetResult();

                // 1. El registro de un formulario válido tiene éxito y devuelve el mensaje.
                if (!resultado.Exito || resultado.Mensaje is null)
                {
                    return false;
                }

                int id = resultado.Mensaje.Id;

                // 2. El mensaje es recuperable desde un contexto independiente por su Id
                //    y sus datos coinciden con lo ingresado (ya recortado), con
                //    Atendido == false.
                using var contextoLectura = fixture.CrearContexto();
                var relectura = contextoLectura.Mensajes.Find(id);

                if (relectura is null)
                {
                    return false;
                }

                return relectura.Nombre == datos.Nombre
                    && relectura.MedioContacto == datos.MedioContacto
                    && relectura.Asunto == datos.Asunto
                    && relectura.Contenido == datos.Contenido
                    && relectura.Atendido == false;
            }, iter: IteracionesMinimas);
    }

    /// <summary>
    /// Genera un modelo de formulario válido: nombre (1–100), asunto (1–150) y
    /// contenido (1–2000) sin espacios al inicio o al final y no vacíos, junto a un
    /// medio de contacto que es un correo válido o un teléfono de 7 a 15 dígitos.
    /// </summary>
    private static Gen<(string Nombre, string MedioContacto, string Asunto, string Contenido)> GenModeloValido() =>
        Gen.Select(
            GenTextoSinEspaciosExtremos(1, 100),
            GenMedioContactoValido(),
            GenTextoSinEspaciosExtremos(1, 150),
            GenTextoSinEspaciosExtremos(1, 2000),
            (nombre, medio, asunto, contenido) => (nombre, medio, asunto, contenido));

    /// <summary>
    /// Genera un texto de longitud entre <paramref name="minimo"/> y
    /// <paramref name="maximo"/> cuyos caracteres no son espacios en blanco, de modo
    /// que el valor es no vacío tras recortar y el recorte lo deja inalterado.
    /// </summary>
    private static Gen<string> GenTextoSinEspaciosExtremos(int minimo, int maximo) =>
        Gen.Int[minimo, maximo]
            .SelectMany(n => CaracterVisible.Array[n])
            .Select(cs => new string(cs));

    /// <summary>
    /// Caracteres visibles sin espacios en blanco: letras, dígitos y algunos signos
    /// de puntuación comunes.
    /// </summary>
    private static readonly Gen<char> CaracterVisible =
        Gen.OneOf(
            Gen.Char['a', 'z'],
            Gen.Char['A', 'Z'],
            Gen.Char['0', '9'],
            Gen.OneOf(
                Gen.Const('.'),
                Gen.Const(','),
                Gen.Const('!'),
                Gen.Const('?'),
                Gen.Const('-'),
                Gen.Const('@')));

    /// <summary>
    /// Genera un medio de contacto válido: con la misma frecuencia un correo con
    /// formato <c>nombre@dominio.extensión</c> o un teléfono de 7 a 15 dígitos.
    /// </summary>
    private static Gen<string> GenMedioContactoValido() =>
        Gen.OneOf(GenCorreoValido(), GenTelefonoValido());

    private static readonly Gen<char> LetraDigito =
        Gen.OneOf(Gen.Char['a', 'z'], Gen.Char['A', 'Z'], Gen.Char['0', '9']);

    private static readonly Gen<string> Etiqueta =
        LetraDigito.Array[1, 6].Select(cs => new string(cs));

    private static readonly Gen<string> Extension =
        Gen.Char['a', 'z'].Array[2, 4].Select(cs => new string(cs));

    /// <summary>Correos con formato correcto <c>nombre@dominio.extensión</c>.</summary>
    private static Gen<string> GenCorreoValido() =>
        Gen.Select(
            LetraDigito.Array[1, 8].Select(cs => new string(cs)),
            Etiqueta.Array[1, 3].Select(ls => string.Join('.', ls)),
            Extension,
            (local, dominio, ext) => $"{local}@{dominio}.{ext}");

    /// <summary>Teléfonos válidos de 7 a 15 dígitos (incluye los límites 7 y 15).</summary>
    private static Gen<string> GenTelefonoValido() =>
        Gen.Int[7, 15]
            .SelectMany(n => Gen.Char['0', '9'].Array[n])
            .Select(cs => new string(cs));
}
