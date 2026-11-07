using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Services;

/// <summary>
/// Servicio de dominio para la gestión de la galería del resguardo. Expone el CRUD
/// de los temas (<see cref="TemaGaleria"/>) y de sus elementos
/// (<see cref="ElementoGaleria"/>), así como la asociación de cada elemento a su
/// tema a través de la clave foránea <see cref="ElementoGaleria.TemaGaleriaId"/>
/// (Requisitos 10, 11, 12, 33, 34, 35 y 36).
/// </summary>
/// <remarks>
/// El guardado físico de los archivos (portadas de temas, fotos y videos) corre por
/// cuenta de <see cref="IArchivoService"/> en la capa de controladores; este
/// servicio solo persiste los valores de <see cref="TemaGaleria.PortadaRuta"/> y
/// <see cref="ElementoGaleria.ArchivoRuta"/> sobre las entidades.
/// </remarks>
public interface IGaleriaService
{
    // --- Temas ---

    /// <summary>
    /// Lista todos los temas de la galería para la vista pública
    /// (<c>Galeria/Index</c>), que muestra cada tema con su portada (Requisito 10).
    /// </summary>
    /// <returns>La lista de temas de la galería.</returns>
    Task<IReadOnlyList<TemaGaleria>> ListarTemas();

    /// <summary>
    /// Obtiene un tema por su identificador incluyendo sus elementos cargados, para
    /// la vista <c>Galeria/Tema</c> que muestra fotos y videos del tema
    /// (Requisito 11).
    /// </summary>
    /// <param name="id">Identificador del tema.</param>
    /// <returns>
    /// El <see cref="TemaGaleria"/> con su colección <see cref="TemaGaleria.Elementos"/>
    /// cargada, o <c>null</c> si no existe.
    /// </returns>
    Task<TemaGaleria?> ObtenerTema(int id);

    /// <summary>
    /// Crea y persiste un nuevo tema de galería (Requisito 33).
    /// </summary>
    /// <param name="tema">Datos del tema a crear.</param>
    /// <returns>El <see cref="TemaGaleria"/> creado con su identificador asignado.</returns>
    Task<TemaGaleria> CrearTema(TemaGaleria tema);

    /// <summary>
    /// Actualiza los datos de un tema existente (Requisito 34).
    /// </summary>
    /// <param name="tema">Tema con el identificador y los datos actualizados.</param>
    /// <returns>El <see cref="TemaGaleria"/> actualizado, o <c>null</c> si no existe.</returns>
    Task<TemaGaleria?> EditarTema(TemaGaleria tema);

    /// <summary>
    /// Elimina un tema por su identificador. Por la relación en cascada, al eliminar
    /// el tema se eliminan también sus elementos (Requisito 34).
    /// </summary>
    /// <param name="id">Identificador del tema a eliminar.</param>
    /// <returns><c>true</c> si se eliminó; <c>false</c> si no existía.</returns>
    Task<bool> EliminarTema(int id);

    // --- Elementos ---

    /// <summary>
    /// Obtiene un elemento por su identificador, para la vista <c>Galeria/Foto</c>
    /// que muestra la foto ampliada con su descripción (Requisito 12).
    /// </summary>
    /// <param name="id">Identificador del elemento.</param>
    /// <returns>El <see cref="ElementoGaleria"/> encontrado, o <c>null</c> si no existe.</returns>
    Task<ElementoGaleria?> ObtenerElemento(int id);

    /// <summary>
    /// Agrega un elemento a la galería asociándolo a su tema mediante
    /// <see cref="ElementoGaleria.TemaGaleriaId"/> (Requisito 35).
    /// </summary>
    /// <param name="elemento">
    /// Elemento a crear, con su <see cref="ElementoGaleria.TemaGaleriaId"/> apuntando
    /// al tema al que pertenece.
    /// </param>
    /// <returns>El <see cref="ElementoGaleria"/> creado con su identificador asignado.</returns>
    Task<ElementoGaleria> AgregarElemento(ElementoGaleria elemento);

    /// <summary>
    /// Actualiza los datos de un elemento existente (Requisito 36).
    /// </summary>
    /// <param name="elemento">Elemento con el identificador y los datos actualizados.</param>
    /// <returns>El <see cref="ElementoGaleria"/> actualizado, o <c>null</c> si no existe.</returns>
    Task<ElementoGaleria?> EditarElemento(ElementoGaleria elemento);

    /// <summary>
    /// Elimina un elemento por su identificador (Requisito 36).
    /// </summary>
    /// <param name="id">Identificador del elemento a eliminar.</param>
    /// <returns><c>true</c> si se eliminó; <c>false</c> si no existía.</returns>
    Task<bool> EliminarElemento(int id);
}
