using Microsoft.AspNetCore.Mvc;
using PagWebCabildo.Services;

namespace PagWebCabildo.Controllers;

/// <summary>
/// Controlador público de Documentos. Muestra los documentos descargables
/// agrupados por categoría y entrega el archivo físico como descarga
/// (Requisitos 13.1, 14.1). No pertenece a ningún Area: vive en el espacio de
/// nombres raíz de controladores del sitio público.
/// </summary>
public class DocumentosController : Controller
{
    private readonly IDocumentoService _documentoService;
    private readonly IArchivoService _archivoService;

    public DocumentosController(
        IDocumentoService documentoService,
        IArchivoService archivoService)
    {
        _documentoService = documentoService;
        _archivoService = archivoService;
    }

    /// <summary>
    /// Lista los documentos agrupados por categoría, con el título, la descripción
    /// y un enlace de descarga por documento (Requisito 13.1).
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var categorias = await _documentoService.ListarPublicasAgrupadas();
        return View(categorias);
    }

    /// <summary>
    /// Entrega el archivo físico de un documento como descarga forzada
    /// (Content-Disposition: attachment) usando <see cref="IArchivoService.Abrir"/>
    /// (Requisito 14.1). Devuelve 404 si el documento no existe o si el archivo
    /// físico asociado no se encuentra.
    /// </summary>
    public async Task<IActionResult> Descargar(int id)
    {
        var documento = await _documentoService.ObtenerDocumento(id);
        if (documento is null)
        {
            return NotFound();
        }

        // Los documentos de categorías privadas no se descargan desde el sitio
        // público: se devuelve 404 para no revelar su existencia.
        if (!await _documentoService.EsDocumentoPublico(id))
        {
            return NotFound();
        }

        ArchivoAbierto abierto;
        try
        {
            abierto = _archivoService.Abrir(documento.ArchivoRuta);
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }

        var nombreDescarga = ConstruirNombreDescarga(documento.Titulo, abierto.Nombre);
        return File(abierto.Stream, abierto.ContentType, nombreDescarga);
    }

    /// <summary>
    /// Construye un nombre amigable para la descarga a partir del título del
    /// documento, conservando la extensión del archivo físico almacenado. Si el
    /// título está vacío, recurre al nombre del archivo físico.
    /// </summary>
    private static string ConstruirNombreDescarga(string titulo, string nombreArchivo)
    {
        if (string.IsNullOrWhiteSpace(titulo))
        {
            return nombreArchivo;
        }

        var extension = Path.GetExtension(nombreArchivo);
        var baseNombre = SanearNombre(titulo.Trim());
        return string.IsNullOrEmpty(extension) ? baseNombre : $"{baseNombre}{extension}";
    }

    /// <summary>
    /// Reemplaza los caracteres no válidos para nombres de archivo por guiones
    /// bajos, evitando nombres de descarga malformados.
    /// </summary>
    private static string SanearNombre(string nombre)
    {
        var invalidos = Path.GetInvalidFileNameChars();
        var caracteres = nombre
            .Select(c => invalidos.Contains(c) ? '_' : c)
            .ToArray();
        return new string(caracteres);
    }
}
