using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Services;

/// <summary>
/// Servicio de contenido para la información de contacto del resguardo. Opera
/// sobre la entidad singleton <see cref="InformacionContacto"/> (un único registro
/// sembrado con <c>Id = 1</c>) que se muestra en la sección pública de Contacto y
/// se edita desde el Panel_Admin (Requisitos 15 y 40).
/// </summary>
public interface IContactoService
{
    /// <summary>
    /// Obtiene el registro único de información de contacto.
    /// </summary>
    /// <returns>
    /// El <see cref="InformacionContacto"/> existente. Si por cualquier razón no
    /// existiera (el <em>seeding</em> garantiza que sí), se crea y persiste uno con
    /// campos vacíos para preservar la invariante de registro único.
    /// </returns>
    Task<InformacionContacto> ObtenerInformacion();

    /// <summary>
    /// Actualiza los datos de contacto sobre el registro singleton.
    /// </summary>
    /// <param name="direccion">Dirección física del resguardo.</param>
    /// <param name="telefono">Teléfono de contacto.</param>
    /// <param name="correoElectronico">Correo electrónico de contacto.</param>
    /// <param name="otrosDatos">Otros datos de contacto (redes, horarios, etc.).</param>
    /// <returns>El <see cref="InformacionContacto"/> actualizado.</returns>
    Task<InformacionContacto> ActualizarInformacion(
        string direccion,
        string telefono,
        string correoElectronico,
        string otrosDatos);
}
