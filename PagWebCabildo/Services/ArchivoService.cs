using Microsoft.AspNetCore.StaticFiles;

namespace PagWebCabildo.Services;

/// <summary>
/// Implementación de <see cref="IArchivoService"/> que almacena archivos en el
/// sistema de archivos bajo <c>wwwroot/uploads</c>, resolviendo las rutas físicas
/// con <see cref="IWebHostEnvironment.WebRootPath"/>.
/// </summary>
public class ArchivoService : IArchivoService
{
    /// <summary>Carpeta base relativa para todos los archivos subidos.</summary>
    private const string CarpetaBase = "uploads";

    /// <summary>
    /// Extensiones permitidas (imágenes, videos y documentos), en minúsculas y con punto.
    /// </summary>
    private static readonly HashSet<string> ExtensionesPermitidas = new(StringComparer.OrdinalIgnoreCase)
    {
        // Imágenes
        ".jpg", ".jpeg", ".png", ".gif", ".webp",
        // Videos
        ".mp4", ".webm",
        // Documentos
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".odt", ".txt",
    };

    private readonly IWebHostEnvironment _entorno;
    private readonly FileExtensionContentTypeProvider _tipoContenidoProvider = new();

    public ArchivoService(IWebHostEnvironment entorno)
    {
        _entorno = entorno;
    }

    /// <summary>Raíz web, tolerando entornos donde <c>WebRootPath</c> aún no está resuelto.</summary>
    private string RaizWeb =>
        string.IsNullOrEmpty(_entorno.WebRootPath)
            ? Path.Combine(_entorno.ContentRootPath, "wwwroot")
            : _entorno.WebRootPath;

    public async Task<string> Guardar(IFormFile archivo, string subcarpeta)
    {
        if (archivo is null || archivo.Length == 0)
        {
            throw new ArgumentException("El archivo está vacío o no se proporcionó.", nameof(archivo));
        }

        var extension = Path.GetExtension(archivo.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !ExtensionesPermitidas.Contains(extension))
        {
            throw new ArgumentException(
                $"La extensión '{extension}' no está permitida. Extensiones válidas: {string.Join(", ", ExtensionesPermitidas)}.",
                nameof(archivo));
        }

        var subcarpetaNormalizada = NormalizarSubcarpeta(subcarpeta);
        var nombreUnico = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";

        var directorioFisico = Path.Combine(RaizWeb, CarpetaBase, subcarpetaNormalizada);
        Directory.CreateDirectory(directorioFisico);

        var rutaFisica = Path.Combine(directorioFisico, nombreUnico);
        await using (var destino = new FileStream(rutaFisica, FileMode.Create, FileAccess.Write))
        {
            await archivo.CopyToAsync(destino);
        }

        // Ruta relativa persistible con barras hacia adelante, p. ej. "uploads/eventos/{guid}.jpg".
        var subcarpetaRelativa = subcarpetaNormalizada.Replace(Path.DirectorySeparatorChar, '/');
        return string.IsNullOrEmpty(subcarpetaRelativa)
            ? $"{CarpetaBase}/{nombreUnico}"
            : $"{CarpetaBase}/{subcarpetaRelativa}/{nombreUnico}";
    }

    public bool Eliminar(string rutaRelativa)
    {
        if (string.IsNullOrWhiteSpace(rutaRelativa))
        {
            return false;
        }

        var rutaFisica = ResolverRutaFisica(rutaRelativa);
        if (!File.Exists(rutaFisica))
        {
            return false;
        }

        File.Delete(rutaFisica);
        return true;
    }

    public string Mover(string rutaRelativa, string subcarpetaDestino)
    {
        if (string.IsNullOrWhiteSpace(rutaRelativa))
        {
            return rutaRelativa;
        }

        var rutaFisicaOrigen = ResolverRutaFisica(rutaRelativa);
        if (!File.Exists(rutaFisicaOrigen))
        {
            // Nada que mover: se conserva la ruta original para no perder la referencia.
            return rutaRelativa;
        }

        var subcarpetaNormalizada = NormalizarSubcarpeta(subcarpetaDestino);
        var nombre = Path.GetFileName(rutaFisicaOrigen);

        var directorioDestino = Path.Combine(RaizWeb, CarpetaBase, subcarpetaNormalizada);
        Directory.CreateDirectory(directorioDestino);

        var rutaFisicaDestino = Path.Combine(directorioDestino, nombre);
        File.Move(rutaFisicaOrigen, rutaFisicaDestino, overwrite: true);

        var subcarpetaRelativa = subcarpetaNormalizada.Replace(Path.DirectorySeparatorChar, '/');
        return string.IsNullOrEmpty(subcarpetaRelativa)
            ? $"{CarpetaBase}/{nombre}"
            : $"{CarpetaBase}/{subcarpetaRelativa}/{nombre}";
    }

    public ArchivoAbierto Abrir(string rutaRelativa)
    {
        if (string.IsNullOrWhiteSpace(rutaRelativa))
        {
            throw new ArgumentException("La ruta relativa es obligatoria.", nameof(rutaRelativa));
        }

        var rutaFisica = ResolverRutaFisica(rutaRelativa);
        if (!File.Exists(rutaFisica))
        {
            throw new FileNotFoundException("El archivo solicitado no existe.", rutaRelativa);
        }

        var nombre = Path.GetFileName(rutaFisica);
        var contentType = _tipoContenidoProvider.TryGetContentType(rutaFisica, out var tipo)
            ? tipo
            : "application/octet-stream";

        var stream = new FileStream(rutaFisica, FileMode.Open, FileAccess.Read, FileShare.Read);
        return new ArchivoAbierto(stream, contentType, nombre);
    }

    /// <summary>
    /// Normaliza la subcarpeta: unifica separadores, descarta componentes vacíos
    /// y previene traversal de directorios (<c>..</c>).
    /// </summary>
    private static string NormalizarSubcarpeta(string subcarpeta)
    {
        if (string.IsNullOrWhiteSpace(subcarpeta))
        {
            return string.Empty;
        }

        var segmentos = subcarpeta
            .Replace('\\', '/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var segmento in segmentos)
        {
            if (segmento == ".." || segmento == ".")
            {
                throw new ArgumentException("La subcarpeta no puede contener componentes de navegación de rutas.", nameof(subcarpeta));
            }
        }

        return segmentos.Length == 0 ? string.Empty : Path.Combine(segmentos);
    }

    /// <summary>
    /// Convierte una ruta relativa persistida (con barras hacia adelante) en una ruta
    /// física dentro de la raíz web, evitando el escape del directorio base.
    /// </summary>
    private string ResolverRutaFisica(string rutaRelativa)
    {
        var relativaNormalizada = rutaRelativa
            .Replace('\\', '/')
            .TrimStart('/');

        var segmentos = relativaNormalizada.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var segmento in segmentos)
        {
            if (segmento == "..")
            {
                throw new ArgumentException("La ruta no puede contener componentes de navegación de rutas.", nameof(rutaRelativa));
            }
        }

        return Path.Combine(RaizWeb, Path.Combine(segmentos));
    }
}
