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
/// Pruebas de integración de extremo a extremo del renderizado de las vistas públicas
/// y de la descarga de documentos (tarea 14.2). Levantan la aplicación real mediante
/// <see cref="AppTestFactory"/> con una base SQLite en memoria sembrada y ejercitan el
/// flujo HTTP completo con un cliente anónimo (sin autenticar).
///
/// <para>
/// El contenido necesario para las vistas de detalle y para la descarga se siembra por
/// prueba resolviendo servicios desde la fábrica
/// (<c>factory.Services.CreateScope()</c> → <see cref="AppDbContext"/> e
/// <see cref="IArchivoService"/>), sin modificar la fábrica compartida. La conexión en
/// memoria mantenida abierta por la fábrica conserva esos datos entre peticiones, de modo
/// que sembrar en la prueba y luego emitir GETs HTTP funciona.
/// </para>
///
/// Validates: Requirements 1.1, 6.1, 9.1, 11.1, 12.1, 14.1, 15.1
/// </summary>
public sealed class RenderYDescargaTests : IClassFixture<AppTestFactory>
{
    private readonly AppTestFactory _factory;

    public RenderYDescargaTests(AppTestFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Requisito 1.1: la página de inicio (Home/Index) renderiza correctamente,
    /// respondiendo 200 con contenido HTML.
    /// </summary>
    [Fact]
    public async Task Inicio_Renderiza_200YHtml()
    {
        var cliente = _factory.CrearClienteSinRedireccion();

        var respuesta = await cliente.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal("text/html", respuesta.Content.Headers.ContentType?.MediaType);
        var html = await respuesta.Content.ReadAsStringAsync();
        Assert.Contains("<html", html, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Requisito 15.1: la sección pública de contacto renderiza (200).
    /// </summary>
    [Fact]
    public async Task Contacto_Renderiza_200()
    {
        var cliente = _factory.CrearClienteSinRedireccion();

        var respuesta = await cliente.GetAsync("/Contacto");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }

    /// <summary>
    /// Requisito 9.1: la vista de detalle de un comunicado publicado renderiza (200).
    /// </summary>
    [Fact]
    public async Task DetalleComunicadoPublicado_Renderiza_200()
    {
        int id;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var comunicado = new Comunicado
            {
                Titulo = "Comunicado de Prueba Render",
                Fecha = new DateTime(2024, 5, 10),
                Contenido = "Contenido del comunicado de prueba para el detalle público.",
                Estado = EstadoComunicado.Publicado
            };
            db.Comunicados.Add(comunicado);
            await db.SaveChangesAsync();
            id = comunicado.Id;
        }

        var cliente = _factory.CrearClienteSinRedireccion();
        var respuesta = await cliente.GetAsync($"/Comunicados/Detalle/{id}");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }

    /// <summary>
    /// Requisito 6.1: la vista de detalle de un evento renderiza (200).
    /// </summary>
    [Fact]
    public async Task DetalleEvento_Renderiza_200()
    {
        int id;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var evento = new Evento
            {
                Titulo = "Evento de Prueba Render",
                Fecha = new DateOnly(2024, 6, 15),
                Hora = new TimeOnly(10, 30),
                Lugar = "Casa comunal",
                Descripcion = "Descripción del evento de prueba para el detalle público.",
                Cancelado = false
            };
            db.Eventos.Add(evento);
            await db.SaveChangesAsync();
            id = evento.Id;
        }

        var cliente = _factory.CrearClienteSinRedireccion();
        var respuesta = await cliente.GetAsync($"/Eventos/Detalle/{id}");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }

    /// <summary>
    /// Requisitos 11.1 y 12.1: la vista de un tema de galería (fotos y videos) y la
    /// vista de una foto ampliada renderizan (200).
    /// </summary>
    [Fact]
    public async Task TemaYFotoGaleria_Renderizan_200()
    {
        int temaId;
        int elementoId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var tema = new TemaGaleria
            {
                Nombre = "Tema de Prueba Render",
                Descripcion = "Descripción del tema de galería de prueba.",
                PortadaRuta = "uploads/galeria/portadas/portada-prueba.jpg"
            };
            db.TemasGaleria.Add(tema);
            await db.SaveChangesAsync();

            var elemento = new ElementoGaleria
            {
                Tipo = TipoElemento.Foto,
                ArchivoRuta = "uploads/galeria/elementos/foto-prueba.jpg",
                Descripcion = "Foto de prueba para la vista ampliada.",
                TemaGaleriaId = tema.Id
            };
            db.ElementosGaleria.Add(elemento);
            await db.SaveChangesAsync();

            temaId = tema.Id;
            elementoId = elemento.Id;
        }

        var cliente = _factory.CrearClienteSinRedireccion();

        var respuestaTema = await cliente.GetAsync($"/Galeria/Tema/{temaId}");
        Assert.Equal(HttpStatusCode.OK, respuestaTema.StatusCode);

        var respuestaFoto = await cliente.GetAsync($"/Galeria/Foto/{elementoId}");
        Assert.Equal(HttpStatusCode.OK, respuestaFoto.StatusCode);
    }

    /// <summary>
    /// Requisito 14.1: la descarga de un documento entrega el archivo físico correcto.
    /// Se siembra una categoría y un documento cuyo <see cref="Documento.ArchivoRuta"/>
    /// apunta a un archivo real guardado vía <see cref="IArchivoService.Guardar"/>. La
    /// respuesta debe ser 200, con el Content-Type del archivo, una cabecera
    /// Content-Disposition de adjunto con el nombre esperado (título saneado + extensión)
    /// y un cuerpo cuyos bytes coinciden con el contenido subido.
    /// </summary>
    [Fact]
    public async Task DescargarDocumento_EntregaArchivoCorrecto()
    {
        var contenidoEsperado = Encoding.UTF8.GetBytes(
            "Contenido de prueba del documento descargable del resguardo.");
        const string titulo = "Acta de Asamblea 2024";
        const string nombreSubido = "acta-asamblea.txt";

        int documentoId;
        string rutaRelativa;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var archivos = scope.ServiceProvider.GetRequiredService<IArchivoService>();

            var formFile = CrearFormFile(nombreSubido, contenidoEsperado, "text/plain");
            rutaRelativa = await archivos.Guardar(formFile, "documentos");

            var categoria = new CategoriaDocumento { Nombre = "Actas de Prueba" };
            db.CategoriasDocumento.Add(categoria);
            await db.SaveChangesAsync();

            var documento = new Documento
            {
                Titulo = titulo,
                Descripcion = "Documento de prueba para la descarga.",
                ArchivoRuta = rutaRelativa,
                CategoriaDocumentoId = categoria.Id
            };
            db.Documentos.Add(documento);
            await db.SaveChangesAsync();
            documentoId = documento.Id;
        }

        try
        {
            var cliente = _factory.CrearClienteSinRedireccion();
            var respuesta = await cliente.GetAsync($"/Documentos/Descargar/{documentoId}");

            // 200 y contenido correcto.
            Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

            // El Content-Type coincide con el del archivo (.txt → text/plain).
            Assert.Equal("text/plain", respuesta.Content.Headers.ContentType?.MediaType);

            // Content-Disposition indica adjunto con el nombre esperado:
            // título saneado + extensión del archivo físico (.txt).
            var disposition = respuesta.Content.Headers.ContentDisposition;
            Assert.NotNull(disposition);
            Assert.Equal("attachment", disposition!.DispositionType);
            var nombreEsperado = $"Acta de Asamblea 2024.txt";
            var nombreEntregado = disposition.FileNameStar ?? disposition.FileName;
            Assert.NotNull(nombreEntregado);
            Assert.Equal(nombreEsperado, nombreEntregado!.Trim('"'));

            // El cuerpo coincide byte a byte con el contenido subido.
            var cuerpo = await respuesta.Content.ReadAsByteArrayAsync();
            Assert.Equal(contenidoEsperado, cuerpo);
        }
        finally
        {
            // Limpieza del artefacto físico creado en wwwroot/uploads.
            using var scope = _factory.Services.CreateScope();
            var archivos = scope.ServiceProvider.GetRequiredService<IArchivoService>();
            archivos.Eliminar(rutaRelativa);
        }
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
}
