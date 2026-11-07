using System.Text.RegularExpressions;

namespace PagWebCabildo.Services;

/// <summary>
/// Implementación de <see cref="IValidacionContactoService"/>. Es la única fuente
/// de verdad para la Property 8 del diseño: un medio de contacto se acepta si y
/// solo si cumple el formato de correo (<c>nombre@dominio.extensión</c>) o el de
/// teléfono (de 7 a 15 dígitos). Cualquier otra cadena se rechaza.
/// </summary>
public sealed partial class ValidacionContactoService : IValidacionContactoService
{
    /// <summary>
    /// Expresión regular para el correo electrónico con formato
    /// <c>nombre@dominio.extensión</c>:
    /// <list type="bullet">
    /// <item>Parte local: uno o más caracteres permitidos (letras, dígitos y
    /// <c>. _ % + -</c>).</item>
    /// <item>Un único <c>@</c>.</item>
    /// <item>Dominio: una o más etiquetas separadas por puntos, cada una con
    /// letras, dígitos o guiones.</item>
    /// <item>Extensión: un punto final seguido de al menos dos letras.</item>
    /// </list>
    /// El uso de <c>\A</c> y <c>\z</c> ancla la coincidencia a toda la cadena,
    /// evitando que un salto de línea final la haga pasar por válida.
    /// </summary>
    [GeneratedRegex(@"\A[A-Za-z0-9._%+\-]+@[A-Za-z0-9\-]+(?:\.[A-Za-z0-9\-]+)*\.[A-Za-z]{2,}\z")]
    private static partial Regex RegexCorreo();

    /// <summary>
    /// Expresión regular para el teléfono: exactamente de 7 a 15 dígitos, sin
    /// ningún otro carácter (ni espacios, ni signos, ni separadores).
    /// </summary>
    [GeneratedRegex(@"\A[0-9]{7,15}\z")]
    private static partial Regex RegexTelefono();

    /// <inheritdoc />
    public bool EsMedioContactoValido(string medio)
    {
        if (string.IsNullOrWhiteSpace(medio))
        {
            return false;
        }

        var valor = medio.Trim();

        return RegexCorreo().IsMatch(valor) || RegexTelefono().IsMatch(valor);
    }
}
