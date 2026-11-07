using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PagWebCabildo.Tests.Integracion;

/// <summary>
/// Utilidades compartidas para las pruebas de integración que necesitan tratar con
/// el token antiforgery de ASP.NET Core y con el inicio de sesión por cookies.
///
/// <para>
/// Los formularios del Panel de Administración incluyen un campo oculto
/// <c>__RequestVerificationToken</c> (emitido por el <i>tag helper</i> de formulario)
/// y acompañan la petición con una cookie antiforgery. Para enviar un POST válido es
/// necesario: (1) hacer GET de la página del formulario, (2) extraer el token oculto
/// del HTML y conservar la cookie antiforgery, y (3) reenviar ambos en el POST. El
/// <see cref="HttpClient"/> creado aquí usa un <see cref="CookieContainer"/>, por lo
/// que las cookies (antiforgery y de autenticación) se conservan automáticamente entre
/// peticiones del mismo cliente.
/// </para>
/// </summary>
public static class ClienteAutenticacion
{
    /// <summary>Nombre del campo oculto del token antiforgery.</summary>
    public const string CampoTokenAntiforgery = "__RequestVerificationToken";

    /// <summary>Ruta del formulario de inicio de sesión del Area Admin.</summary>
    public const string RutaLogin = "/Admin/Account/Login";

    private static readonly Regex TokenRegex = new(
        @"name=""__RequestVerificationToken""[^>]*\bvalue=""(?<token>[^""]+)""",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Crea un <see cref="HttpClient"/> que NO sigue redirecciones automáticamente
    /// (para poder inspeccionar los 302 y su cabecera <c>Location</c>) y conserva
    /// cookies entre peticiones.
    /// </summary>
    public static HttpClient CrearClienteSinRedireccion(this AppTestFactory factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

    /// <summary>
    /// Extrae el valor del token antiforgery oculto del HTML de un formulario.
    /// </summary>
    public static string ExtraerTokenAntiforgery(string html)
    {
        var coincidencia = TokenRegex.Match(html);
        if (!coincidencia.Success)
        {
            throw new InvalidOperationException(
                "No se encontró el campo __RequestVerificationToken en el HTML del formulario.");
        }

        return coincidencia.Groups["token"].Value;
    }

    /// <summary>
    /// Realiza GET de la página de login, extrae el token antiforgery y envía el POST
    /// de inicio de sesión con las credenciales dadas. El <paramref name="cliente"/>
    /// conserva la cookie de autenticación resultante en su <see cref="CookieContainer"/>.
    /// Devuelve la respuesta del POST (sin seguir la posible redirección).
    /// </summary>
    public static async Task<HttpResponseMessage> IniciarSesion(
        this HttpClient cliente, string usuario, string contraseña)
    {
        var respuestaGet = await cliente.GetAsync(RutaLogin);
        respuestaGet.EnsureSuccessStatusCode();
        var html = await respuestaGet.Content.ReadAsStringAsync();
        var token = ExtraerTokenAntiforgery(html);

        var campos = new Dictionary<string, string>
        {
            [CampoTokenAntiforgery] = token,
            ["Usuario"] = usuario,
            ["Contraseña"] = contraseña
        };

        return await cliente.PostAsync(RutaLogin, new FormUrlEncodedContent(campos));
    }
}
