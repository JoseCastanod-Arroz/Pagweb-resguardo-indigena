using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Services;

/// <summary>
/// Servicio de dominio para la gestión de <see cref="Acta"/> de asamblea del
/// cabildo. Las actas son documentos privados que nunca se exponen en el sitio
/// público; su gestión y descarga solo corresponden a los roles autorizados
/// (Requisitos 3.1, 3.2, 3.3, 3.4, 4.1, 4.2).
/// </summary>
/// <remarks>
/// A diferencia de otros servicios de contenido, este servicio sí coordina el
/// almacenamiento físico del adjunto a través de <see cref="IArchivoService"/>:
/// guarda el archivo en la subcarpeta <c>actas</c> de <c>wwwroot/uploads</c> al
/// crear o al reemplazarlo al editar, y lo abre para su descarga. En la entidad
/// solo se persiste la ruta relativa en <see cref="Acta.ArchivoRuta"/>.
///
/// Reglas de validación aplicadas en <see cref="Crear"/> y <see cref="Editar"/>:
/// <list type="bullet">
///   <item>El consecutivo debe ser positivo y único (Requisito 3.2).</item>
///   <item>La fecha de la asamblea es obligatoria (Requisito 3.2).</item>
///   <item>El tipo de asamblea debe ser un valor válido (Requisito 3.2).</item>
///   <item>El título no puede estar vacío (Requisito 3.2).</item>
/// </list>
/// Las validaciones fallidas se señalan con <see cref="ArgumentException"/> y un
/// mensaje en español para que el controlador lo muestre en el formulario.
/// </remarks>
public interface IActaService
{
    /// <summary>
    /// Lista todas las actas ordenadas de la más reciente a la más antigua: por
    /// <see cref="Acta.FechaAsamblea"/> descendente y, como desempate, por
    /// <see cref="Acta.Id"/> descendente (Requisito 3.1). Incluye el administrador
    /// que registró cada acta para su presentación.
    /// </summary>
    /// <returns>Las actas del más reciente al más antiguo.</returns>
    Task<IReadOnlyList<Acta>> Listar();

    /// <summary>
    /// Obtiene un acta por su identificador, incluyendo el administrador que la
    /// registró.
    /// </summary>
    /// <param name="id">Identificador del acta.</param>
    /// <returns>El <see cref="Acta"/> encontrado, o <c>null</c> si no existe.</returns>
    Task<Acta?> Obtener(int id);

    /// <summary>
    /// Crea y persiste un acta nueva. Valida el consecutivo, la fecha, el tipo y el
    /// título; guarda el archivo adjunto en <c>uploads/actas</c> vía
    /// <see cref="IArchivoService"/>, fija <see cref="Acta.ArchivoRuta"/> y
    /// establece <see cref="Acta.FechaRegistro"/> en la fecha y hora actuales. El
    /// archivo es obligatorio al crear.
    /// </summary>
    /// <param name="acta">Datos del acta a crear.</param>
    /// <param name="archivo">Archivo adjunto del acta (obligatorio).</param>
    /// <returns>El <see cref="Acta"/> creado con su identificador asignado.</returns>
    /// <exception cref="ArgumentException">
    /// Si alguna validación falla (consecutivo no positivo o duplicado, fecha
    /// ausente, tipo inválido, título vacío) o si el archivo es nulo/vacío o de
    /// extensión no permitida. El mensaje describe el problema para el formulario.
    /// </exception>
    Task<Acta> Crear(Acta acta, IFormFile archivo);

    /// <summary>
    /// Actualiza los datos de un acta existente. Valida el consecutivo, la fecha,
    /// el tipo y el título. Si se proporciona un nuevo <paramref name="archivo"/>,
    /// lo guarda en <c>uploads/actas</c>, actualiza <see cref="Acta.ArchivoRuta"/>
    /// y elimina el archivo anterior; si <paramref name="archivo"/> es <c>null</c>,
    /// conserva el adjunto existente.
    /// </summary>
    /// <param name="acta">Acta con el identificador y los datos actualizados.</param>
    /// <param name="archivo">Nuevo archivo adjunto, o <c>null</c> para conservar el existente.</param>
    /// <returns>El <see cref="Acta"/> actualizado, o <c>null</c> si no existe.</returns>
    /// <exception cref="ArgumentException">
    /// Si alguna validación falla (consecutivo no positivo o duplicado en otra
    /// acta, fecha ausente, tipo inválido, título vacío) o si el archivo
    /// proporcionado es de extensión no permitida.
    /// </exception>
    Task<Acta?> Editar(Acta acta, IFormFile? archivo);

    /// <summary>
    /// Abre el archivo adjunto de un acta para su descarga.
    /// </summary>
    /// <param name="id">Identificador del acta.</param>
    /// <returns>
    /// El <see cref="ArchivoAbierto"/> con el flujo, el tipo de contenido y el
    /// nombre del archivo, o <c>null</c> si el acta no existe o no tiene archivo
    /// adjunto, para que el controlador devuelva 404.
    /// </returns>
    /// <exception cref="FileNotFoundException">Si el archivo físico no existe.</exception>
    Task<ArchivoAbierto?> AbrirArchivo(int id);
}
