using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Services;

/// <summary>
/// Servicio de dominio para la veeduría (fiscalización) del cabildo. Registra las
/// acciones relevantes realizadas por los administradores sobre los módulos de
/// contenido (crear, editar, eliminar, publicar) y expone su consulta de solo
/// lectura para el rol Fiscal y el rol Gobernador (Requisitos 5.1, 5.2, 5.3).
/// </summary>
/// <remarks>
/// La escritura del registro de actividad es resiliente: nunca debe interrumpir
/// la operación principal del llamador. Si el registro falla, el error se anota
/// en el log pero la acción de contenido se considera completada (ver manejo de
/// errores del diseño). Por ello <see cref="Registrar"/> no propaga excepciones.
/// </remarks>
public interface IRegistroActividadService
{
    /// <summary>
    /// Registra una acción realizada por un administrador sobre un módulo de
    /// contenido, fijando la fecha y hora del registro (Requisito 5.1).
    /// </summary>
    /// <remarks>
    /// Es resiliente por diseño: si la persistencia del registro falla, el error
    /// se anota en el log y el método retorna sin lanzar excepción, de modo que la
    /// operación de contenido del llamador no se vea interrumpida.
    /// </remarks>
    /// <param name="administradorId">Identificador del administrador que realizó la acción.</param>
    /// <param name="accion">Acción realizada: Crear, Editar, Eliminar o Publicar.</param>
    /// <param name="modulo">Módulo sobre el que se realizó la acción (Comunicado, Evento, Acta, etc.).</param>
    /// <param name="detalle">Detalle descriptivo de la acción registrada.</param>
    Task Registrar(int administradorId, string accion, string modulo, string detalle);

    /// <summary>
    /// Lista los registros de actividad ordenados por fecha y hora de forma
    /// descendente (más reciente primero), para su consulta de solo lectura en la
    /// veeduría (Requisitos 5.2, 5.3).
    /// </summary>
    /// <returns>Los registros de actividad del más reciente al más antiguo.</returns>
    Task<IReadOnlyList<RegistroActividad>> Listar();
}
