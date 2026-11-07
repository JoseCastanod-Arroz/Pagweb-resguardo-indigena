using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using PagWebCabildo.Services;
using Xunit;

namespace PagWebCabildo.Tests.Services;

/// <summary>
/// Pruebas unitarias de <see cref="ArchivoService"/> (implementación de
/// <see cref="IArchivoService"/>). Verifican el guardado con extensión válida,
/// el rechazo de extensiones no permitidas, la eliminación del archivo físico y
/// la apertura de un archivo guardado.
///
/// Cada prueba usa una raíz web temporal aislada (<see cref="EntornoWebFalso"/>)
/// que se elimina al finalizar mediante <see cref="IDisposable"/>.
///
/// Validates: Requirements File/Media Handling
/// </summary>
public sealed class ArchivoServiceTests : IDisposable
{
    private readonly EntornoWebFalso _entorno;
    private readonly ArchivoService _servicio;

    public ArchivoServiceTests()
    {
        _entorno = new EntornoWebFalso();
        _servicio = new ArchivoService(_entorno);
    }

    public void Dispose() => _entorno.Dispose();

    [Fact]
    public async Task Guardar_ConExtensionValida_EscribeArchivoYDevuelveRutaRelativa()
    {
        var archivo = CrearArchivo("foto.jpg", "contenido-de-imagen");

        var rutaRelativa = await _servicio.Guardar(archivo, "eventos");

        // La ruta devuelta usa barras hacia adelante y cuelga de uploads/{subcarpeta}.
        Assert.StartsWith("uploads/eventos/", rutaRelativa);
        Assert.EndsWith(".jpg", rutaRelativa);
        Assert.DoesNotContain('\\', rutaRelativa);

        // El archivo físico existe bajo la raíz web.
        var rutaFisica = RutaFisicaDesde(rutaRelativa);
        Assert.True(File.Exists(rutaFisica), "El archivo físico debería existir tras guardarlo.");
        Assert.Equal("contenido-de-imagen", await File.ReadAllTextAsync(rutaFisica));
    }

    [Theory]
    [InlineData("malware.exe")]
    [InlineData("script.sh")]
    [InlineData("archivo.zip")]
    [InlineData("sin-extension")]
    public async Task Guardar_ConExtensionNoPermitida_LanzaArgumentExceptionYNoEscribeNada(string nombre)
    {
        var archivo = CrearArchivo(nombre, "contenido");

        await Assert.ThrowsAsync<ArgumentException>(() => _servicio.Guardar(archivo, "documentos"));

        // No debe haberse creado ningún archivo bajo uploads.
        var carpetaUploads = Path.Combine(_entorno.WebRootPath, "uploads");
        if (Directory.Exists(carpetaUploads))
        {
            Assert.Empty(Directory.GetFiles(carpetaUploads, "*", SearchOption.AllDirectories));
        }
    }

    [Fact]
    public async Task Guardar_ArchivoVacio_LanzaArgumentException()
    {
        var archivo = CrearArchivo("vacio.jpg", string.Empty);

        await Assert.ThrowsAsync<ArgumentException>(() => _servicio.Guardar(archivo, "eventos"));
    }

    [Fact]
    public async Task Eliminar_ArchivoGuardado_LoBorraYDevuelveTrue()
    {
        var archivo = CrearArchivo("documento.pdf", "contenido-pdf");
        var rutaRelativa = await _servicio.Guardar(archivo, "documentos");
        var rutaFisica = RutaFisicaDesde(rutaRelativa);
        Assert.True(File.Exists(rutaFisica));

        var eliminado = _servicio.Eliminar(rutaRelativa);

        Assert.True(eliminado);
        Assert.False(File.Exists(rutaFisica), "El archivo físico debería haberse eliminado.");
    }

    [Fact]
    public void Eliminar_RutaInexistente_DevuelveFalse()
    {
        var eliminado = _servicio.Eliminar("uploads/documentos/no-existe.pdf");

        Assert.False(eliminado);
    }

    [Fact]
    public async Task Abrir_ArchivoGuardado_DevuelveContenidoYTipoCorrectos()
    {
        var archivo = CrearArchivo("reporte.pdf", "cuerpo-del-pdf");
        var rutaRelativa = await _servicio.Guardar(archivo, "documentos");

        var abierto = _servicio.Abrir(rutaRelativa);

        using (abierto.Stream)
        {
            using var lector = new StreamReader(abierto.Stream);
            Assert.Equal("cuerpo-del-pdf", await lector.ReadToEndAsync());
        }

        Assert.Equal("application/pdf", abierto.ContentType);
        Assert.EndsWith(".pdf", abierto.Nombre);
    }

    [Fact]
    public void Abrir_RutaInexistente_LanzaFileNotFoundException()
    {
        Assert.Throws<FileNotFoundException>(() => _servicio.Abrir("uploads/documentos/no-existe.pdf"));
    }

    /// <summary>Construye un <see cref="IFormFile"/> en memoria con el nombre y contenido dados.</summary>
    private static IFormFile CrearArchivo(string nombre, string contenido)
    {
        var bytes = Encoding.UTF8.GetBytes(contenido);
        var flujo = new MemoryStream(bytes);
        return new FormFile(flujo, 0, flujo.Length, "archivo", nombre);
    }

    /// <summary>Resuelve la ruta física a partir de una ruta relativa con barras hacia adelante.</summary>
    private string RutaFisicaDesde(string rutaRelativa)
    {
        var segmentos = rutaRelativa.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return Path.Combine(_entorno.WebRootPath, Path.Combine(segmentos));
    }

    /// <summary>
    /// <see cref="IWebHostEnvironment"/> falso cuyo <c>WebRootPath</c> apunta a un
    /// directorio temporal único. Al liberarse, elimina de forma recursiva ese
    /// directorio para no dejar residuos en el sistema de archivos.
    /// </summary>
    private sealed class EntornoWebFalso : IWebHostEnvironment, IDisposable
    {
        public EntornoWebFalso()
        {
            ContentRootPath = Path.Combine(Path.GetTempPath(), "PagWebCabildoTests_" + Guid.NewGuid().ToString("N"));
            WebRootPath = Path.Combine(ContentRootPath, "wwwroot");
            Directory.CreateDirectory(WebRootPath);

            WebRootFileProvider = new PhysicalFileProvider(WebRootPath);
            ContentRootFileProvider = new PhysicalFileProvider(ContentRootPath);
        }

        public string WebRootPath { get; set; }
        public IFileProvider WebRootFileProvider { get; set; }
        public string ApplicationName { get; set; } = "PagWebCabildo.Tests";
        public IFileProvider ContentRootFileProvider { get; set; }
        public string ContentRootPath { get; set; }
        public string EnvironmentName { get; set; } = "Testing";

        public void Dispose()
        {
            (WebRootFileProvider as IDisposable)?.Dispose();
            (ContentRootFileProvider as IDisposable)?.Dispose();

            try
            {
                if (Directory.Exists(ContentRootPath))
                {
                    Directory.Delete(ContentRootPath, recursive: true);
                }
            }
            catch (IOException)
            {
                // Mejor esfuerzo: si el SO retiene algún identificador, se ignora en limpieza.
            }
        }
    }
}
