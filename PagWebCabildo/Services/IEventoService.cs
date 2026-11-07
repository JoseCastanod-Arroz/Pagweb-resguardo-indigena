using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Services;

/// <summary>
/// Servicio de dominio para la gestión de <see cref="Evento"/>. Expone las
/// operaciones CRUD, el listado ordenado por proximidad de fecha para el sitio
/// público, el marcado de un evento como cancelado y la asociación de la ruta de
/// imagen a la entidad (Requisitos 5.1, 5.2, 6, 7, 26, 27, 28.1 y 29).
/// </summary>
/// <remarks>
/// El guardado físico de los archivos de imagen corre por cuenta de
/// <see cref="IArchivoService"/> en la capa de controladores; este servicio solo
/// persiste el valor de <see cref="Evento.ImagenRuta"/> sobre la entidad.
/// </remarks>
public interface IEventoService
{
    /// <summary>
    /// Lista todos los eventos ordenados por proximidad de fecha: primero los de
    /// fecha más próxima y, a igualdad de fecha, los de hora más temprana.
    /// </summary>
    /// <returns>
    /// La lista de eventos ordenada ascendentemente por <see cref="Evento.Fecha"/>
    /// y, como desempate, por <see cref="Evento.Hora"/>. Es la única fuente de
    /// verdad para la Property 1 del diseño (Requisito 5.2).
    /// </returns>
    Task<IReadOnlyList<Evento>> ListarOrdenadosPorProximidad();

    /// <summary>
    /// Obtiene un evento por su identificador, para la vista de detalle.
    /// </summary>
    /// <param name="id">Identificador del evento.</param>
    /// <returns>El <see cref="Evento"/> encontrado, o <c>null</c> si no existe.</returns>
    Task<Evento?> Obtener(int id);

    /// <summary>
    /// Crea y persiste un nuevo evento.
    /// </summary>
    /// <param name="evento">Datos del evento a crear.</param>
    /// <returns>El <see cref="Evento"/> creado con su identificador asignado.</returns>
    Task<Evento> Crear(Evento evento);

    /// <summary>
    /// Actualiza los datos de un evento existente.
    /// </summary>
    /// <param name="evento">Evento con el identificador y los datos actualizados.</param>
    /// <returns>El <see cref="Evento"/> actualizado, o <c>null</c> si no existe.</returns>
    Task<Evento?> Editar(Evento evento);

    /// <summary>
    /// Elimina un evento por su identificador.
    /// </summary>
    /// <param name="id">Identificador del evento a eliminar.</param>
    /// <returns><c>true</c> si se eliminó; <c>false</c> si no existía.</returns>
    Task<bool> Eliminar(int id);

    /// <summary>
    /// Marca un evento como cancelado estableciendo <see cref="Evento.Cancelado"/>
    /// en <c>true</c> (Requisito 29). El evento permanece visible y recuperable.
    /// </summary>
    /// <param name="id">Identificador del evento a cancelar.</param>
    /// <returns>El <see cref="Evento"/> cancelado, o <c>null</c> si no existe.</returns>
    Task<Evento?> MarcarCancelado(int id);

    /// <summary>
    /// Asocia (o limpia) la ruta de imagen de un evento, persistiendo el valor de
    /// <see cref="Evento.ImagenRuta"/>. No realiza ninguna operación de E/S de
    /// archivos; el guardado físico corresponde a <see cref="IArchivoService"/>.
    /// </summary>
    /// <param name="id">Identificador del evento.</param>
    /// <param name="imagenRuta">
    /// Ruta relativa de la imagen a asociar, o <c>null</c> para desvincular la
    /// imagen existente.
    /// </param>
    /// <returns>El <see cref="Evento"/> actualizado, o <c>null</c> si no existe.</returns>
    Task<Evento?> AsociarImagen(int id, string? imagenRuta);
}
