namespace PagWebCabildo.Models.Entidades;

/// <summary>
/// Registro de una acción realizada por un administrador sobre un módulo de contenido.
/// Sirve para la veeduría (fiscalización) del cabildo.
/// </summary>
public class RegistroActividad
{
    public int Id { get; set; }

    /// <summary>
    /// Administrador que realizó la acción.
    /// </summary>
    public int AdministradorId { get; set; }

    /// <summary>
    /// Acción realizada: Crear, Editar, Eliminar o Publicar.
    /// </summary>
    public string Accion { get; set; } = string.Empty;

    /// <summary>
    /// Módulo sobre el que se realizó la acción: Comunicado, Evento, Acta, etc.
    /// </summary>
    public string Modulo { get; set; } = string.Empty;

    /// <summary>
    /// Detalle descriptivo de la acción registrada.
    /// </summary>
    public string Detalle { get; set; } = string.Empty;

    /// <summary>
    /// Fecha y hora en que se registró la actividad.
    /// </summary>
    public DateTime FechaHora { get; set; }
}
