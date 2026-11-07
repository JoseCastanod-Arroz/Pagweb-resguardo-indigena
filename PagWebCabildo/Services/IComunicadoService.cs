using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Services;

/// <summary>
/// Servicio de dominio para la gestión de <see cref="Comunicado"/>. Encapsula el
/// CRUD, el listado público ordenado del más reciente al más antiguo, las reglas
/// de visibilidad de borradores y las transiciones de estado (guardar como
/// borrador y publicar).
///
/// Aplica las reglas de negocio del dominio de comunicados:
/// <list type="bullet">
///   <item>El listado público solo expone comunicados en estado
///   <see cref="EstadoComunicado.Publicado"/>, ordenados por fecha descendente
///   (Requisitos 8.1, 8.2; Propiedades 2 y 3).</item>
///   <item>Los borradores nunca se exponen por listado ni por acceso directo en
///   el sitio público (Requisitos 32.3; Propiedad 3).</item>
///   <item>Guardar como borrador exige título y contenido (Requisito 32.2).</item>
///   <item>Publicar solo procede desde el estado borrador (Requisitos 32.4,
///   32.5, 32.6; Propiedad 11).</item>
/// </list>
/// </summary>
public interface IComunicadoService
{
    // ----- Listado y acceso público (Sitio_Publico) -----

    /// <summary>
    /// Lista los comunicados visibles en el sitio público: únicamente los que
    /// están en estado <see cref="EstadoComunicado.Publicado"/>, ordenados del
    /// más reciente al más antiguo (fecha descendente).
    /// </summary>
    /// <remarks>
    /// Fuente única de verdad del orden público (Propiedad 2, Requisito 8.2) y del
    /// ocultamiento de borradores en listados (Propiedad 3, Requisitos 8.1, 32.3).
    /// </remarks>
    /// <returns>Comunicados publicados ordenados por fecha descendente.</returns>
    Task<IReadOnlyList<Comunicado>> ListarPublicadosRecientesPrimero();

    /// <summary>
    /// Obtiene un comunicado para el sitio público por su identificador, pero solo
    /// si está en estado <see cref="EstadoComunicado.Publicado"/>.
    /// </summary>
    /// <remarks>
    /// Garantiza que un borrador no sea accesible por enlace directo en el sitio
    /// público (Propiedad 3, Requisito 32.3): devuelve <c>null</c> si el comunicado
    /// no existe o si está en estado borrador.
    /// </remarks>
    /// <param name="id">Identificador del comunicado.</param>
    /// <returns>El comunicado publicado, o <c>null</c> si no existe o es borrador.</returns>
    Task<Comunicado?> ObtenerPublicado(int id);

    // ----- Consultas de administración (Panel_Admin) -----

    /// <summary>
    /// Lista todos los comunicados sin filtrar por estado, para la gestión desde el
    /// Panel_Admin. Se ordenan del más reciente al más antiguo por consistencia con
    /// el sitio público.
    /// </summary>
    /// <returns>Todos los comunicados (borradores y publicados).</returns>
    Task<IReadOnlyList<Comunicado>> ListarTodos();

    /// <summary>
    /// Obtiene un comunicado por su identificador independientemente de su estado,
    /// para edición y gestión desde el Panel_Admin.
    /// </summary>
    /// <param name="id">Identificador del comunicado.</param>
    /// <returns>El comunicado, o <c>null</c> si no existe.</returns>
    Task<Comunicado?> Obtener(int id);

    // ----- CRUD -----

    /// <summary>
    /// Crea un comunicado. El estado se toma del parámetro proporcionado; para
    /// crear directamente un borrador validado use <see cref="GuardarBorrador"/>.
    /// </summary>
    /// <param name="titulo">Título del comunicado.</param>
    /// <param name="fecha">Fecha del comunicado.</param>
    /// <param name="contenido">Contenido del comunicado.</param>
    /// <param name="estado">Estado inicial del comunicado.</param>
    /// <returns>El comunicado creado y persistido.</returns>
    Task<Comunicado> Crear(string titulo, DateTime fecha, string contenido, EstadoComunicado estado);

    /// <summary>
    /// Edita los datos de un comunicado existente (título, fecha y contenido). No
    /// modifica el estado; las transiciones de estado se realizan con
    /// <see cref="GuardarBorrador"/> y <see cref="Publicar"/>.
    /// </summary>
    /// <param name="id">Identificador del comunicado a editar.</param>
    /// <param name="titulo">Nuevo título.</param>
    /// <param name="fecha">Nueva fecha.</param>
    /// <param name="contenido">Nuevo contenido.</param>
    /// <returns>El comunicado actualizado.</returns>
    /// <exception cref="InvalidOperationException">Si el comunicado no existe.</exception>
    Task<Comunicado> Editar(int id, string titulo, DateTime fecha, string contenido);

    /// <summary>
    /// Elimina un comunicado por su identificador.
    /// </summary>
    /// <param name="id">Identificador del comunicado a eliminar.</param>
    /// <returns><c>true</c> si existía y se eliminó; <c>false</c> si no existía.</returns>
    Task<bool> Eliminar(int id);

    // ----- Transiciones de estado -----

    /// <summary>
    /// Guarda un comunicado como borrador. Exige título y contenido no vacíos
    /// (Requisito 32.2). Si se indica <paramref name="id"/>, actualiza el
    /// comunicado existente; en caso contrario crea uno nuevo. El estado resultante
    /// es siempre <see cref="EstadoComunicado.Borrador"/>.
    /// </summary>
    /// <param name="id">Identificador del comunicado a actualizar, o <c>null</c> para crear uno nuevo.</param>
    /// <param name="titulo">Título del comunicado (obligatorio).</param>
    /// <param name="fecha">Fecha del comunicado.</param>
    /// <param name="contenido">Contenido del comunicado (obligatorio).</param>
    /// <returns>El comunicado guardado como borrador.</returns>
    /// <exception cref="ArgumentException">
    /// Si el título o el contenido están vacíos o en blanco. El mensaje indica los
    /// campos obligatorios faltantes para que el controlador pueda mostrarlos
    /// (Requisito 32.2).
    /// </exception>
    /// <exception cref="InvalidOperationException">Si se indica un <paramref name="id"/> que no existe.</exception>
    Task<Comunicado> GuardarBorrador(int? id, string titulo, DateTime fecha, string contenido);

    /// <summary>
    /// Publica un comunicado. La operación solo procede cuando el comunicado está
    /// en estado <see cref="EstadoComunicado.Borrador"/>; en cualquier otro estado
    /// se rechaza y el estado permanece sin cambios (Propiedad 11, Requisitos 32.4,
    /// 32.5, 32.6).
    /// </summary>
    /// <param name="id">Identificador del comunicado a publicar.</param>
    /// <returns>El comunicado ya publicado.</returns>
    /// <exception cref="InvalidOperationException">
    /// Si el comunicado no existe, o si no está en estado borrador (no puede
    /// publicarse desde su estado actual).
    /// </exception>
    Task<Comunicado> Publicar(int id);
}
