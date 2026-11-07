using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Services;

/// <summary>
/// Servicio de contenido para el texto de presentación/historia del resguardo.
/// Opera sobre la entidad singleton <see cref="PresentacionResguardo"/> (un único
/// registro sembrado con <c>Id = 1</c>) que alimenta la sección de Historia del
/// sitio público y su edición desde el Panel_Admin (Requisitos 3 y 24).
/// </summary>
public interface IPresentacionService
{
    /// <summary>
    /// Obtiene el registro único de presentación del resguardo.
    /// </summary>
    /// <returns>
    /// El <see cref="PresentacionResguardo"/> existente. Si por cualquier razón no
    /// existiera (el <em>seeding</em> garantiza que sí), se crea y persiste uno con
    /// texto vacío para preservar la invariante de registro único.
    /// </returns>
    Task<PresentacionResguardo> ObtenerPresentacion();

    /// <summary>
    /// Actualiza el texto de presentación del resguardo sobre el registro singleton.
    /// </summary>
    /// <param name="texto">Nuevo texto de presentación/historia a persistir.</param>
    /// <returns>El <see cref="PresentacionResguardo"/> actualizado.</returns>
    Task<PresentacionResguardo> ActualizarPresentacion(string texto);
}
