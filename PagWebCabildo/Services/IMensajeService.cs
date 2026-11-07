using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Services;

/// <summary>
/// Resultado de un intento de registro de un <see cref="Mensaje"/> a través del
/// formulario de contacto. Hace inequívoco si el mensaje se registró o no, de modo
/// que las Propiedades 9 y 10 del diseño puedan verificarse sin ambigüedad:
/// <list type="bullet">
///   <item>Cuando <see cref="Exito"/> es <c>false</c>, <see cref="Mensaje"/> es
///   <c>null</c> y no se persistió ningún registro (Propiedad 9, Req 16.4); los
///   motivos se enumeran en <see cref="Errores"/>.</item>
///   <item>Cuando <see cref="Exito"/> es <c>true</c>, <see cref="Mensaje"/> contiene
///   el registro ya persistido y recuperable, con datos que coinciden con los
///   ingresados (Propiedad 10, Req 16.3, 17.1); <see cref="Errores"/> está vacío.</item>
/// </list>
/// </summary>
/// <param name="Exito">Indica si el mensaje se registró correctamente.</param>
/// <param name="Errores">Lista de mensajes de error cuando el registro se rechaza.</param>
/// <param name="Mensaje">El mensaje persistido cuando el registro tiene éxito; <c>null</c> en caso contrario.</param>
public record ResultadoRegistroMensaje(bool Exito, IReadOnlyList<string> Errores, Mensaje? Mensaje);

/// <summary>
/// Servicio de dominio para la gestión de <see cref="Mensaje"/> del formulario de
/// contacto. Encapsula el registro validado desde el sitio público y las operaciones
/// de administración (listado del más reciente al más antiguo, detalle, marcar como
/// atendido y eliminación).
///
/// Reglas de negocio:
/// <list type="bullet">
///   <item>El registro exige los cuatro campos obligatorios (nombre, medio de
///   contacto, asunto y contenido) no vacíos; un campo vacío o compuesto solo por
///   espacios rechaza el envío sin registrar nada (Propiedad 9, Req 16.4).</item>
///   <item>Se aplican los límites de longitud: nombre 1–100, asunto 1–150,
///   contenido 1–2000 (Req 16.1).</item>
///   <item>El medio de contacto debe cumplir el formato de correo o teléfono según
///   <see cref="IValidacionContactoService"/> (Req 16.2, 16.5).</item>
///   <item>El listado de administración se ordena por fecha de recepción descendente
///   (Propiedad 4, Req 41.2).</item>
/// </list>
/// </summary>
public interface IMensajeService
{
    /// <summary>
    /// Límite máximo de caracteres del nombre.
    /// </summary>
    const int NombreMaxLongitud = 100;

    /// <summary>
    /// Límite máximo de caracteres del asunto.
    /// </summary>
    const int AsuntoMaxLongitud = 150;

    /// <summary>
    /// Límite máximo de caracteres del contenido del mensaje.
    /// </summary>
    const int ContenidoMaxLongitud = 2000;

    /// <summary>
    /// Valida y registra un mensaje enviado desde el formulario de contacto.
    /// </summary>
    /// <remarks>
    /// Reglas de validación (Propiedades 9 y 10; Req 16.3, 16.4, 17.1):
    /// <list type="bullet">
    ///   <item>Los cuatro campos son obligatorios; vacíos o solo con espacios se
    ///   rechazan sin registrar ningún mensaje.</item>
    ///   <item>Longitudes: nombre 1–<see cref="NombreMaxLongitud"/>, asunto
    ///   1–<see cref="AsuntoMaxLongitud"/>, contenido 1–<see cref="ContenidoMaxLongitud"/>.</item>
    ///   <item>El medio de contacto debe pasar
    ///   <see cref="IValidacionContactoService.EsMedioContactoValido(string)"/>.</item>
    /// </list>
    /// Cuando alguna regla no se cumple, no se persiste nada y el resultado indica el
    /// fallo con la lista de errores. Cuando todas se cumplen, el mensaje se persiste
    /// con <see cref="Mensaje.FechaRecepcion"/> en el momento del registro y
    /// <see cref="Mensaje.Atendido"/> en <c>false</c>, y se devuelve en el resultado.
    /// Los valores de texto se recortan (<c>Trim</c>) antes de persistir.
    /// </remarks>
    /// <param name="nombre">Nombre del visitante (obligatorio, 1–100 caracteres).</param>
    /// <param name="medioContacto">Medio de contacto: correo o teléfono válido (obligatorio).</param>
    /// <param name="asunto">Asunto del mensaje (obligatorio, 1–150 caracteres).</param>
    /// <param name="contenido">Cuerpo del mensaje (obligatorio, 1–2000 caracteres).</param>
    /// <returns>
    /// Un <see cref="ResultadoRegistroMensaje"/> que indica el éxito o fracaso del
    /// registro. En caso de éxito incluye el mensaje persistido; en caso de fracaso,
    /// la lista de errores y <c>null</c> como mensaje.
    /// </returns>
    Task<ResultadoRegistroMensaje> RegistrarMensaje(string nombre, string medioContacto, string asunto, string contenido);

    /// <summary>
    /// Lista todos los mensajes para el Panel_Admin ordenados del más reciente al
    /// más antiguo por <see cref="Mensaje.FechaRecepcion"/>.
    /// </summary>
    /// <remarks>
    /// Fuente única de verdad del orden del listado de mensajes (Propiedad 4,
    /// Req 41.1, 41.2): <c>OrderByDescending(m =&gt; m.FechaRecepcion)</c>.
    /// </remarks>
    /// <returns>Los mensajes ordenados por fecha de recepción descendente.</returns>
    Task<IReadOnlyList<Mensaje>> ListarRecientesPrimero();

    /// <summary>
    /// Obtiene un mensaje por su identificador para el detalle de administración
    /// (Req 42.1).
    /// </summary>
    /// <param name="id">Identificador del mensaje.</param>
    /// <returns>El mensaje, o <c>null</c> si no existe.</returns>
    Task<Mensaje?> Ver(int id);

    /// <summary>
    /// Marca un mensaje como atendido (Req 42.2).
    /// </summary>
    /// <param name="id">Identificador del mensaje.</param>
    /// <returns>El mensaje actualizado, o <c>null</c> si no existe.</returns>
    Task<Mensaje?> MarcarAtendido(int id);

    /// <summary>
    /// Elimina un mensaje por su identificador (Req 43.1).
    /// </summary>
    /// <param name="id">Identificador del mensaje a eliminar.</param>
    /// <returns><c>true</c> si existía y se eliminó; <c>false</c> si no existía.</returns>
    Task<bool> Eliminar(int id);
}
