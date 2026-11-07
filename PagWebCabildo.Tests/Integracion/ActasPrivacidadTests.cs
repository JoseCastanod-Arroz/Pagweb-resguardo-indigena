using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using PagWebCabildo.Data;
using PagWebCabildo.Models.Entidades;
using PagWebCabildo.Services;
using Xunit;

namespace PagWebCabildo.Tests.Integracion;

/// <summary>
/// Prueba de integración de extremo a extremo de la privacidad de las actas (tarea 5.3).
/// Levanta la aplicación real mediante <see cref="AppTestFactory"/> con una base SQLite
/// en memoria sembrada y ejercita el flujo HTTP completo con un cliente ANÓNIMO (sin
/// autenticar), verificando que ninguna ruta del sitio público expone un acta ni su
/// archivo adjunto.
///
/// <para>
/// El acta de prueba se siembra resolviendo servicios desde la fábrica
/// (<c>factory.Services.CreateScope()</c> → <see cref="AppDbContext"/> e
/// <see cref="IArchivoService"/>), guardando un adjunto real en
/// <c>wwwroot/uploads/actas</c> y reutilizando un <see cref="Administrador"/> ya
/// sembrado como <see cref="Acta.RegistradoPorId"/> (la FK usa ON DELETE RESTRICT).
/// El artefacto físico se limpia en un bloque <c>finally</c>.
/// </para>
///
/// Feature: roles-cabildo-actas, Property 1: Las actas nunca son accesibles desde el
/// sitio público. "Ninguna ruta del Sitio_Publico expone un acta ni su archivo mediante
/// enlace." Las actas se gestionan SOLO en el Area Admin (ActasController) y su descarga
/// exige la política ConsultaActas (AdministradorGeneral, Secretario, Fiscal) vía
/// /Admin/Actas/Descargar.
///
/// Validates: Requirements 4.3
/// </summary>
public sealed class ActasPrivacidadTests : IClassFixture<AppTestFactory>
{
    private const string RutaLogin = "/Admin/Account/Login";

    private readonly AppTestFactory _factory;

    public ActasPrivacidadTests(AppTestFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Property 1 / Requisito 4.3: ninguna ruta pública (ni del Area Admin sin
    /// autenticar) expone un acta ni su archivo adjunto a un usuario anónimo.
    /// </summary>
    [Fact]
    public async Task ActaYSuArchivo_NoSonAccesiblesDesdeElSitioPublico()
    {
        // ----------------------------------------------------------------------------
        // Arrange: sembrar un acta con un adjunto real en wwwroot/uploads/actas,
        // reutilizando un administrador existente como RegistradoPor (FK RESTRICT).
        // ----------------------------------------------------------------------------
        var contenidoAdjunto = Encoding.UTF8.GetBytes(
            "Contenido confidencial del acta de asamblea del cabildo. No debe ser público.");
        const string tituloActa = "Acta Privada de Asamblea Confidencial 2024";
        const string nombreSubido = "acta-asamblea.pdf";

        int actaId;
        string rutaRelativaArchivo;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var archivos = scope.ServiceProvider.GetRequiredService<IArchivoService>();

            // Guardar el adjunto físico real en uploads/actas (igual que lo hace el servicio real).
            var formFile = CrearFormFile(nombreSubido, contenidoAdjunto, "application/pdf");
            rutaRelativaArchivo = await archivos.Guardar(formFile, "actas");

            // Reutilizar un administrador sembrado (admin / AdministradorGeneral) como
            // RegistradoPorId: la FK Acta → Administrador usa ON DELETE RESTRICT.
            var administradorId = db.Administradores
                .OrderBy(a => a.Id)
                .Select(a => a.Id)
                .First();

            var acta = new Acta
            {
                Consecutivo = 1,
                FechaAsamblea = new DateOnly(2024, 7, 20),
                Tipo = TipoAsamblea.Ordinaria,
                Titulo = tituloActa,
                Descripcion = "Acta de prueba para verificar la privacidad frente al sitio público.",
                ArchivoRuta = rutaRelativaArchivo,
                RegistradoPorId = administradorId,
                FechaRegistro = DateTime.UtcNow
            };
            db.Actas.Add(acta);
            await db.SaveChangesAsync();
            actaId = acta.Id;
        }

        try
        {
            // Cliente ANÓNIMO (sin iniciar sesión).
            var cliente = _factory.CrearClienteSinRedireccion();

            // ------------------------------------------------------------------------
            // Assert 1: las rutas del Area Admin de actas NO son accesibles sin login.
            // Deben redirigir al login (302 → /Admin/Account/Login) o denegar (401/403),
            // nunca devolver 200 con el contenido del acta.
            // ------------------------------------------------------------------------
            var respuestaIndexAdmin = await cliente.GetAsync("/Admin/Actas");
            AssertNoExponeActaAdmin(respuestaIndexAdmin);

            var respuestaDescargaAdmin = await cliente.GetAsync($"/Admin/Actas/Descargar/{actaId}");
            AssertNoExponeActaAdmin(respuestaDescargaAdmin);

            // ------------------------------------------------------------------------
            // Assert 2: NO existe ningún controlador público (fuera de Admin) que sirva
            // actas. En esta app MVC, un controlador/acción inexistente produce 404
            // (ruta por defecto: {controller=Home}/{action=Index}/{id?}).
            // ------------------------------------------------------------------------
            await AssertRutaPublicaNoExponeActa(cliente, "/Actas", tituloActa);
            await AssertRutaPublicaNoExponeActa(cliente, "/Actas/Index", tituloActa);
            await AssertRutaPublicaNoExponeActa(cliente, $"/Actas/Descargar/{actaId}", tituloActa);
            await AssertRutaPublicaNoExponeActa(cliente, $"/Actas/Detalle/{actaId}", tituloActa);

            // ------------------------------------------------------------------------
            // Assert 3: el archivo físico del acta (uploads/actas/{guid}.ext) NO debe
            // servirse directamente a un cliente anónimo. La descarga legítima pasa solo
            // por la acción protegida /Admin/Actas/Descargar (cubierta por Assert 1).
            //
            // NOTA: si esta aserción falla (el archivo se sirve con 200), se trata de una
            // violación REAL de la Property 1 / Requisito 4.3: el adjunto del acta sería
            // accesible públicamente por su ruta estática. La aserción se mantiene
            // expresando la expectativa correcta de privacidad para documentar la brecha.
            // ------------------------------------------------------------------------
            var rutaEstaticaCruda = "/" + rutaRelativaArchivo.TrimStart('/');
            var respuestaArchivoCrudo = await cliente.GetAsync(rutaEstaticaCruda);

            Assert.True(
                respuestaArchivoCrudo.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Forbidden
                    or HttpStatusCode.Unauthorized,
                $"El adjunto del acta en la ruta estática '{rutaEstaticaCruda}' NO debe servirse a un " +
                $"cliente anónimo (Property 1 / Requisito 4.3), pero respondió {(int)respuestaArchivoCrudo.StatusCode}.");

            // Refuerzo: aunque la respuesta no fuera 404/403, el cuerpo NO debe contener
            // el contenido confidencial del adjunto.
            if (respuestaArchivoCrudo.StatusCode == HttpStatusCode.OK)
            {
                var cuerpoArchivo = await respuestaArchivoCrudo.Content.ReadAsByteArrayAsync();
                Assert.False(
                    cuerpoArchivo.SequenceEqual(contenidoAdjunto),
                    "El contenido del adjunto del acta se entregó por la ruta estática pública " +
                    "(violación de la Property 1 / Requisito 4.3).");
            }
        }
        finally
        {
            // ----------------------------------------------------------------------------
            // Limpieza del artefacto físico creado en wwwroot/uploads/actas.
            // ----------------------------------------------------------------------------
            using var scope = _factory.Services.CreateScope();
            var archivos = scope.ServiceProvider.GetRequiredService<IArchivoService>();
            archivos.Eliminar(rutaRelativaArchivo);
        }
    }

    /// <summary>
    /// Afirma que una ruta del Area Admin de actas NO expone el acta a un cliente
    /// anónimo: debe redirigir al login (302 → /Admin/Account/Login) o denegar
    /// (401/403), nunca responder 200.
    /// </summary>
    private static void AssertNoExponeActaAdmin(HttpResponseMessage respuesta)
    {
        if (respuesta.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
        {
            return;
        }

        Assert.True(EsRedireccion(respuesta.StatusCode),
            $"Se esperaba redirección a login o 401/403, pero fue {(int)respuesta.StatusCode}.");

        var location = respuesta.Headers.Location?.ToString() ?? string.Empty;
        Assert.Contains(RutaLogin, location, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Afirma que una ruta pública (fuera del Area Admin) NO expone un acta: se espera
    /// 404 para el controlador público inexistente "Actas". Si el enrutamiento de la app
    /// difiere, al menos la respuesta NO debe ser 200 ni contener el título del acta.
    /// </summary>
    private static async Task AssertRutaPublicaNoExponeActa(
        HttpClient cliente, string ruta, string tituloActa)
    {
        var respuesta = await cliente.GetAsync(ruta);

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);

        // Refuerzo independiente del código: el cuerpo nunca debe contener el acta.
        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        Assert.DoesNotContain(tituloActa, cuerpo, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Crea un <see cref="IFormFile"/> en memoria con el nombre, contenido y tipo dados,
    /// apto para pasar a <see cref="IArchivoService.Guardar"/> al sembrar.
    /// </summary>
    private static IFormFile CrearFormFile(string nombreArchivo, byte[] contenido, string contentType)
    {
        var stream = new MemoryStream(contenido);
        return new FormFile(stream, 0, contenido.Length, "archivo", nombreArchivo)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    private static bool EsRedireccion(HttpStatusCode codigo) =>
        (int)codigo is >= 300 and < 400;
}
