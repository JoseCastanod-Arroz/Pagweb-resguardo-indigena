using PagWebCabildo.Models.Entidades;
using PagWebCabildo.Models.ViewModels;

namespace PagWebCabildo.Services;

/// <summary>
/// Servicio de dominio del foro comunitario. Gestiona las publicaciones
/// (recetas e historias) y los comentarios anónimos, incluyendo la moderación:
/// el contenido lo envían visitantes y solo es público una vez aprobado por un
/// administrador. La E/S física de las imágenes corresponde a
/// <see cref="IArchivoService"/> en la capa de controladores; este servicio solo
/// persiste la ruta relativa en <see cref="PublicacionForo.ImagenRuta"/>.
/// </summary>
public interface IForoService
{
    // ----- Consultas públicas (solo contenido aprobado) -----

    /// <summary>
    /// Devuelve una página del feed público del foro (solo publicaciones
    /// aprobadas), con el número de comentarios aprobados de cada publicación.
    /// Permite buscar por texto (título, contenido o autor), filtrar por tipo,
    /// ordenar por recientes o más comentados, y paginar. Pensado para escalar
    /// cuando el foro tenga muchas publicaciones.
    /// </summary>
    /// <param name="busqueda">Término a buscar. Si es nulo o vacío, no filtra por texto.</param>
    /// <param name="tipo">Tipo a filtrar. Si es nulo, incluye recetas e historias.</param>
    /// <param name="orden">Criterio de ordenamiento (recientes o más comentados).</param>
    /// <param name="pagina">Página a devolver (1-based).</param>
    /// <param name="tamañoPagina">Número de elementos por página.</param>
    Task<ForoFeedPaginado> ListarAprobadas(
        string? busqueda = null,
        TipoPublicacionForo? tipo = null,
        OrdenForo orden = OrdenForo.Recientes,
        int pagina = 1,
        int tamañoPagina = 10);

    /// <summary>
    /// Obtiene una publicación aprobada con sus comentarios aprobados, para el
    /// detalle público. Devuelve <c>null</c> si no existe o no está aprobada.
    /// </summary>
    Task<PublicacionForo?> ObtenerAprobadaConComentarios(int id);

    // ----- Envío por visitantes anónimos -----

    /// <summary>
    /// Registra una publicación enviada por un visitante. Queda en estado
    /// <see cref="EstadoPublicacionForo.Pendiente"/> y con la fecha de envío.
    /// </summary>
    Task<PublicacionForo> CrearPublicacion(PublicacionForo publicacion);

    /// <summary>
    /// Registra un comentario de un visitante sobre una publicación aprobada.
    /// Queda pendiente de moderación. Devuelve <c>null</c> si la publicación no
    /// existe o no está aprobada.
    /// </summary>
    Task<ComentarioForo?> CrearComentario(ComentarioForo comentario);

    // ----- Moderación (administración) -----

    /// <summary>Lista todas las publicaciones (cualquier estado) para moderar.</summary>
    Task<IReadOnlyList<PublicacionForo>> ListarTodasPublicaciones();

    /// <summary>Lista los comentarios pendientes de moderación con su publicación.</summary>
    Task<IReadOnlyList<ComentarioForo>> ListarComentariosPendientes();

    /// <summary>Obtiene una publicación por id con todos sus comentarios.</summary>
    Task<PublicacionForo?> ObtenerPublicacionConComentarios(int id);

    /// <summary>Obtiene una publicación por id (sin comentarios).</summary>
    Task<PublicacionForo?> ObtenerPublicacion(int id);

    /// <summary>Cambia el estado de moderación de una publicación. Devuelve <c>null</c> si no existe.</summary>
    Task<PublicacionForo?> CambiarEstadoPublicacion(int id, EstadoPublicacionForo estado);

    /// <summary>Elimina una publicación (y en cascada sus comentarios). Devuelve <c>false</c> si no existe.</summary>
    Task<bool> EliminarPublicacion(int id);

    /// <summary>Cambia el estado de moderación de un comentario. Devuelve <c>null</c> si no existe.</summary>
    Task<ComentarioForo?> CambiarEstadoComentario(int id, EstadoPublicacionForo estado);

    /// <summary>Elimina un comentario. Devuelve <c>false</c> si no existe.</summary>
    Task<bool> EliminarComentario(int id);
}
