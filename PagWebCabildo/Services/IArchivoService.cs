namespace PagWebCabildo.Services;

/// <summary>
/// Resultado de abrir un archivo para descarga: flujo de datos, tipo de contenido y nombre del archivo.
/// </summary>
public record ArchivoAbierto(Stream Stream, string ContentType, string Nombre);

/// <summary>
/// Servicio transversal para el manejo de archivos subidos (imágenes de eventos,
/// portadas de galería, fotos/videos y documentos). Los archivos se almacenan en
/// <c>wwwroot/uploads</c> y en las entidades solo se persiste la ruta relativa.
/// </summary>
public interface IArchivoService
{
    /// <summary>
    /// Guarda un archivo subido en la subcarpeta indicada de <c>wwwroot/uploads</c>,
    /// generando un nombre único (<see cref="System.Guid"/> + extensión original) y
    /// validando la extensión contra el conjunto de tipos permitidos.
    /// </summary>
    /// <param name="archivo">Archivo recibido del formulario.</param>
    /// <param name="subcarpeta">Subcarpeta relativa dentro de uploads, p. ej. <c>eventos</c> o <c>galeria/portadas</c>.</param>
    /// <returns>La ruta relativa persistible, p. ej. <c>uploads/eventos/{guid}.jpg</c> (con barras hacia adelante).</returns>
    /// <exception cref="ArgumentException">Si el archivo es nulo/vacío o la extensión no está permitida.</exception>
    Task<string> Guardar(IFormFile archivo, string subcarpeta);

    /// <summary>
    /// Elimina el archivo físico correspondiente a una ruta relativa.
    /// </summary>
    /// <param name="rutaRelativa">Ruta relativa persistida en la entidad, p. ej. <c>uploads/eventos/{guid}.jpg</c>.</param>
    /// <returns><c>true</c> si el archivo existía y se eliminó; <c>false</c> en caso contrario.</returns>
    bool Eliminar(string rutaRelativa);

    /// <summary>
    /// Mueve un archivo ya almacenado a otra subcarpeta de <c>wwwroot/uploads</c>,
    /// conservando su nombre. Útil para reubicar un documento cuando su categoría
    /// pasa de pública a privada o viceversa.
    /// </summary>
    /// <param name="rutaRelativa">Ruta relativa actual del archivo.</param>
    /// <param name="subcarpetaDestino">Subcarpeta destino dentro de uploads.</param>
    /// <returns>La nueva ruta relativa persistible. Si el archivo de origen no existe, devuelve la ruta original sin cambios.</returns>
    string Mover(string rutaRelativa, string subcarpetaDestino);

    /// <summary>
    /// Abre un archivo para descarga.
    /// </summary>
    /// <param name="rutaRelativa">Ruta relativa persistida en la entidad.</param>
    /// <returns>El flujo del archivo, su tipo de contenido y el nombre del archivo.</returns>
    /// <exception cref="FileNotFoundException">Si el archivo no existe.</exception>
    ArchivoAbierto Abrir(string rutaRelativa);
}
