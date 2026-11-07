using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace PagWebCabildo.Tests.Integracion;

/// <summary>
/// Pruebas de integración de extremo a extremo de autenticación y autorización del
/// Panel de Administración (tarea 14.1). Levantan la aplicación real mediante
/// <see cref="AppTestFactory"/> con una base SQLite en memoria sembrada y ejercitan
/// el flujo HTTP completo (cookies, antiforgery, redirecciones y políticas de rol).
///
/// Validates: Requirements 18.1, 18.2, 18.3, 23.5, 23.6
/// </summary>
public sealed class AutenticacionAutorizacionTests : IClassFixture<AppTestFactory>
{
    private const string RutaLogin = "/Admin/Account/Login";
    private const string RutaDashboard = "/Admin/Dashboard";
    private const string RutaUsuarios = "/Admin/Usuarios";
    private const string RutaAccesoDenegado = "/Admin/Account/AccesoDenegado";

    private readonly AppTestFactory _factory;

    public AutenticacionAutorizacionTests(AppTestFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Requisito 18.3: un usuario no autenticado que intenta acceder a una pantalla
    /// del Panel_Admin es redirigido al inicio de sesión sin exponer el contenido.
    /// </summary>
    [Fact]
    public async Task AccesoNoAutenticado_AlAreaAdmin_RedirigeALogin()
    {
        var cliente = _factory.CrearClienteSinRedireccion();

        var respuesta = await cliente.GetAsync(RutaDashboard);

        Assert.True(EsRedireccion(respuesta.StatusCode),
            $"Se esperaba una redirección, pero fue {(int)respuesta.StatusCode}.");

        var location = respuesta.Headers.Location?.ToString() ?? string.Empty;
        Assert.Contains(RutaLogin, location, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Requisito 18.2: con credenciales inválidas no se crea sesión; se permanece en
    /// la pantalla de login (200) y no se redirige al panel ni se emite cookie de auth.
    /// </summary>
    [Fact]
    public async Task Login_CredencialesInvalidas_MantieneEnLoginSinSesion()
    {
        var cliente = _factory.CrearClienteSinRedireccion();

        var respuesta = await cliente.IniciarSesion(
            AppTestFactory.AdminUsuario, "contraseña-incorrecta");

        // Vuelve a mostrar el formulario (200), no redirige al Dashboard.
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.False(EsRedireccion(respuesta.StatusCode));

        // No se emitió cookie de autenticación: acceder al Dashboard sigue redirigiendo.
        var dashboard = await cliente.GetAsync(RutaDashboard);
        Assert.True(EsRedireccion(dashboard.StatusCode));
        var location = dashboard.Headers.Location?.ToString() ?? string.Empty;
        Assert.Contains(RutaLogin, location, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Requisito 18.1: con credenciales válidas de AdministradorGeneral se crea una
    /// sesión autenticada (cookie) y se redirige a la pantalla principal (Dashboard).
    /// </summary>
    [Fact]
    public async Task Login_CredencialesValidas_RedirigeADashboardYCreaSesion()
    {
        var cliente = _factory.CrearClienteSinRedireccion();

        var respuesta = await cliente.IniciarSesion(
            AppTestFactory.AdminUsuario, AppTestFactory.AdminContraseña);

        // Redirige (302) al Dashboard.
        Assert.True(EsRedireccion(respuesta.StatusCode),
            $"Se esperaba redirección tras login válido, pero fue {(int)respuesta.StatusCode}.");
        var location = respuesta.Headers.Location?.ToString() ?? string.Empty;
        Assert.Contains("Dashboard", location, StringComparison.OrdinalIgnoreCase);

        // Se emitió cookie de autenticación.
        Assert.True(
            respuesta.Headers.TryGetValues("Set-Cookie", out var cookies) &&
            cookies.Any(c => c.StartsWith(".AspNetCore.Cookies", StringComparison.OrdinalIgnoreCase)),
            "El login válido debe emitir la cookie de autenticación.");

        // La sesión funciona: ahora el Dashboard responde 200 (no redirige).
        var dashboard = await cliente.GetAsync(RutaDashboard);
        Assert.Equal(HttpStatusCode.OK, dashboard.StatusCode);
    }

    /// <summary>
    /// Requisito 23.5: un Editor autenticado que intenta acceder a la gestión de
    /// cuentas es rechazado y redirigido a la pantalla de acceso denegado (política
    /// GestionCuentas), sin exponer el módulo.
    /// </summary>
    [Fact]
    public async Task Editor_GestionDeCuentas_EsRechazado()
    {
        var cliente = _factory.CrearClienteSinRedireccion();
        await IniciarSesionEditor(cliente);

        var respuesta = await cliente.GetAsync(RutaUsuarios);

        AssertRechazado(respuesta);
    }

    /// <summary>
    /// Requisito 23.6: un Editor autenticado que intenta eliminar contenido (POST a
    /// Autoridades/Eliminar) es rechazado y redirigido a la pantalla de acceso denegado
    /// (política EliminarContenido); el contenido se conserva.
    /// </summary>
    [Fact]
    public async Task Editor_EliminarContenido_EsRechazado()
    {
        var cliente = _factory.CrearClienteSinRedireccion();
        await IniciarSesionEditor(cliente);

        // Obtener un token antiforgery válido desde una página del panel accesible al
        // Editor (el listado de autoridades) para enviar el POST de eliminación.
        var html = await (await cliente.GetAsync("/Admin/Autoridades")).Content.ReadAsStringAsync();
        var token = ClienteAutenticacion.ExtraerTokenAntiforgery(html);

        var campos = new Dictionary<string, string>
        {
            [ClienteAutenticacion.CampoTokenAntiforgery] = token
        };

        var respuesta = await cliente.PostAsync(
            $"/Admin/Autoridades/Eliminar/{_factory.AutoridadEjemploId}",
            new FormUrlEncodedContent(campos));

        AssertRechazado(respuesta);
    }

    /// <summary>Inicia sesión como el Editor sembrado y verifica que la sesión se creó.</summary>
    private static async Task IniciarSesionEditor(HttpClient cliente)
    {
        var login = await cliente.IniciarSesion(
            AppTestFactory.EditorUsuario, AppTestFactory.EditorContraseña);

        Assert.True(EsRedireccion(login.StatusCode),
            "El Editor debe poder iniciar sesión (redirección tras login válido).");
    }

    /// <summary>
    /// Afirma que una respuesta representa un rechazo de autorización: o bien un 403,
    /// o bien una redirección a la pantalla de acceso denegado. En ningún caso el
    /// Editor debe recibir el contenido del módulo restringido (200 de ese módulo).
    /// </summary>
    private static void AssertRechazado(HttpResponseMessage respuesta)
    {
        if (respuesta.StatusCode == HttpStatusCode.Forbidden)
        {
            return;
        }

        Assert.True(EsRedireccion(respuesta.StatusCode),
            $"Se esperaba 403 o redirección a acceso denegado, pero fue {(int)respuesta.StatusCode}.");

        var location = respuesta.Headers.Location?.ToString() ?? string.Empty;
        Assert.Contains(RutaAccesoDenegado, location, StringComparison.OrdinalIgnoreCase);
    }

    private static bool EsRedireccion(HttpStatusCode codigo) =>
        (int)codigo is >= 300 and < 400;
}
