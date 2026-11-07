using System;
using System.Linq;
using CsCheck;
using PagWebCabildo.Services;
using Xunit;

namespace PagWebCabildo.Tests.Services;

/// <summary>
/// Prueba basada en propiedades de la Property 8 del diseño para el servicio de
/// validación del medio de contacto.
///
/// Validates: Requirements 16.2, 16.5
///
/// El servicio bajo prueba es
/// <see cref="ValidacionContactoService.EsMedioContactoValido(string)"/>. Según el
/// Requisito 16.2, un medio de contacto es válido si y solo si es un correo con
/// formato <c>nombre@dominio.extensión</c> o un número de teléfono de 7 a 15
/// dígitos; cualquier otra cadena se rechaza (Requisito 16.5). El servicio recorta
/// los espacios al inicio y al final antes de validar.
///
/// La prueba genera categorías mixtas de entradas (correos válidos, teléfonos
/// válidos en los límites 7 y 15, teléfonos inválidos, correos mal formados y
/// cadenas aleatorias o vacías) y verifica la relación bicondicional usando un
/// oráculo independiente derivado de la definición del requisito, no de la
/// expresión regular interna del servicio.
/// </summary>
public class ValidacionContactoPropertyTests
{
    private const int IteracionesMinimas = 100;

    // Feature: sitio-web-resguardo, Property 8: El medio de contacto se acepta si y solo si es correo o teléfono válidos
    [Fact]
    public void EsMedioContactoValido_AceptaSiYSoloSiEsCorreoOTelefonoValido()
    {
        var servicio = new ValidacionContactoService();

        GenMedioContacto()
            .Sample(medio =>
            {
                bool resultado = servicio.EsMedioContactoValido(medio);
                bool esperado = OraculoEsValido(medio);
                return resultado == esperado;
            }, iter: IteracionesMinimas);
    }

    /// <summary>
    /// Oráculo independiente que decide la expectativa a partir de la definición
    /// del Requisito 16.2, replicando la regla "recortar y aceptar si es correo o
    /// teléfono válido" mediante lógica distinta a la del servicio.
    /// </summary>
    private static bool OraculoEsValido(string? medio)
    {
        if (string.IsNullOrWhiteSpace(medio))
        {
            return false;
        }

        var valor = medio.Trim();
        return EsCorreoValido(valor) || EsTelefonoValido(valor);
    }

    /// <summary>
    /// Teléfono válido: entre 7 y 15 caracteres, todos dígitos decimales.
    /// </summary>
    private static bool EsTelefonoValido(string valor)
    {
        if (valor.Length < 7 || valor.Length > 15)
        {
            return false;
        }

        return valor.All(c => c >= '0' && c <= '9');
    }

    /// <summary>
    /// Correo válido con formato <c>nombre@dominio.extensión</c>, evaluado sin
    /// expresiones regulares para mantener el oráculo independiente del servicio.
    /// </summary>
    private static bool EsCorreoValido(string valor)
    {
        int arroba = valor.IndexOf('@');
        if (arroba <= 0 || arroba != valor.LastIndexOf('@'))
        {
            // Debe haber exactamente un '@' y no al inicio.
            return false;
        }

        string local = valor[..arroba];
        string dominio = valor[(arroba + 1)..];

        if (local.Length == 0 || dominio.Length == 0)
        {
            return false;
        }

        // Parte local: letras, dígitos y . _ % + -
        foreach (char c in local)
        {
            if (!(EsAlfanumerico(c) || c is '.' or '_' or '%' or '+' or '-'))
            {
                return false;
            }
        }

        // El dominio debe terminar con ".extensión" de al menos dos letras.
        int ultimoPunto = dominio.LastIndexOf('.');
        if (ultimoPunto <= 0 || ultimoPunto == dominio.Length - 1)
        {
            return false;
        }

        string extension = dominio[(ultimoPunto + 1)..];
        if (extension.Length < 2 || !extension.All(EsLetra))
        {
            return false;
        }

        // Etiquetas del dominio (incluida la que precede a la extensión):
        // letras, dígitos o guiones, y ninguna vacía.
        string etiquetas = dominio[..ultimoPunto];
        foreach (string etiqueta in etiquetas.Split('.'))
        {
            if (etiqueta.Length == 0)
            {
                return false;
            }

            foreach (char c in etiqueta)
            {
                if (!(EsAlfanumerico(c) || c == '-'))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool EsLetra(char c) => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');

    private static bool EsAlfanumerico(char c) => EsLetra(c) || (c >= '0' && c <= '9');

    // --- Generadores de categorías de entrada ---

    private static Gen<string> GenMedioContacto() =>
        Gen.OneOf(
            GenCorreoValido(),
            GenTelefonoValido(),
            GenTelefonoInvalido(),
            GenCorreoMalFormado(),
            GenCadenaAleatoria());

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

    /// <summary>
    /// Teléfonos válidos de 7 a 15 dígitos, incluyendo con frecuencia los límites
    /// de longitud 7 y 15.
    /// </summary>
    private static Gen<string> GenTelefonoValido() =>
        Gen.Int[7, 15]
            .SelectMany(n => Gen.Char['0', '9'].Array[n])
            .Select(cs => new string(cs));

    /// <summary>
    /// Teléfonos inválidos: longitudes fuera del rango (6 o 16 dígitos) y dígitos
    /// mezclados con espacios o símbolos.
    /// </summary>
    private static Gen<string> GenTelefonoInvalido() =>
        Gen.OneOf(
            // 6 dígitos (por debajo del mínimo).
            Gen.Char['0', '9'].Array[6].Select(cs => new string(cs)),
            // 16 dígitos (por encima del máximo).
            Gen.Char['0', '9'].Array[16].Select(cs => new string(cs)),
            // Dígitos con un espacio intercalado (7 a 15 caracteres en total).
            Gen.Int[7, 15].SelectMany(n => Gen.Char['0', '9'].Array[n])
                .Select(cs => new string(cs).Insert(cs.Length / 2, " ")),
            // Dígitos con un signo + delante.
            Gen.Int[7, 15].SelectMany(n => Gen.Char['0', '9'].Array[n])
                .Select(cs => "+" + new string(cs)));

    /// <summary>
    /// Correos mal formados: sin <c>@</c>, sin extensión, con parte local o
    /// dominio vacíos, o con doble <c>@</c>.
    /// </summary>
    private static Gen<string> GenCorreoMalFormado() =>
        Gen.OneOf(
            // Sin '@'.
            Gen.Select(Etiqueta, Extension, (d, e) => $"{d}.{e}"),
            // Sin extensión final.
            Gen.Select(
                LetraDigito.Array[1, 6].Select(cs => new string(cs)),
                Etiqueta,
                (l, d) => $"{l}@{d}"),
            // Parte local vacía.
            Gen.Select(Etiqueta, Extension, (d, e) => $"@{d}.{e}"),
            // Dominio vacío.
            LetraDigito.Array[1, 6].Select(cs => new string(cs) + "@"),
            // Doble '@'.
            Gen.Select(
                LetraDigito.Array[1, 6].Select(cs => new string(cs)),
                Etiqueta,
                Extension,
                (l, d, e) => $"{l}@@{d}.{e}"));

    /// <summary>
    /// Cadenas arbitrarias, incluidas vacías, con espacios y caracteres variados.
    /// </summary>
    private static Gen<string> GenCadenaAleatoria() =>
        Gen.OneOf(
            Gen.Const(string.Empty),
            Gen.Const("   "),
            Gen.String[0, 20]);
}
