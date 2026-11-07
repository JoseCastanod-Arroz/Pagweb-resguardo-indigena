using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Services;

/// <summary>
/// Servicio de gestión de las autoridades, cargos y comités del resguardo
/// (Requisitos 4 y 25). Ofrece las operaciones CRUD y el reordenamiento, y
/// garantiza que el listado se devuelva ordenado de forma ascendente por el
/// campo <see cref="Autoridad.Orden"/>.
/// </summary>
public interface IAutoridadService
{
    /// <summary>
    /// Lista todas las autoridades ordenadas de forma ascendente por su campo
    /// <see cref="Autoridad.Orden"/>. Este orden es la única fuente de verdad de
    /// la Property 5 del diseño ("Las autoridades se muestran en el orden
    /// definido", Requisitos 4.2 y 25.3).
    /// </summary>
    /// <returns>Las autoridades ordenadas ascendentemente por <c>Orden</c>.</returns>
    Task<IReadOnlyList<Autoridad>> Listar();

    /// <summary>
    /// Obtiene una autoridad por su identificador.
    /// </summary>
    /// <param name="id">Identificador de la autoridad.</param>
    /// <returns>La autoridad encontrada, o <c>null</c> si no existe.</returns>
    Task<Autoridad?> Obtener(int id);

    /// <summary>
    /// Crea una nueva autoridad (Requisito 25.1).
    /// </summary>
    /// <param name="autoridad">Autoridad a crear.</param>
    /// <returns>La autoridad creada, con su identificador asignado.</returns>
    Task<Autoridad> Crear(Autoridad autoridad);

    /// <summary>
    /// Edita una autoridad existente (Requisito 25.2).
    /// </summary>
    /// <param name="autoridad">Autoridad con los datos actualizados.</param>
    Task Editar(Autoridad autoridad);

    /// <summary>
    /// Elimina una autoridad por su identificador (Requisito 25.5).
    /// </summary>
    /// <param name="id">Identificador de la autoridad a eliminar.</param>
    Task Eliminar(int id);

    /// <summary>
    /// Reordena las autoridades indicadas asignando a cada una el valor de
    /// <see cref="Autoridad.Orden"/> suministrado (Requisito 25.4).
    /// </summary>
    /// <param name="ordenes">
    /// Pares <c>(id, orden)</c> con el nuevo valor de <c>Orden</c> para cada
    /// autoridad identificada por <c>id</c>.
    /// </param>
    Task ReordenarAutoridades(IEnumerable<(int id, int orden)> ordenes);
}
