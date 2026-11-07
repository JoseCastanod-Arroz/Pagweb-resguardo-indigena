namespace PagWebCabildo.Models.Entidades;

/// <summary>
/// Mensaje enviado por un visitante a través del formulario de contacto.
/// </summary>
public class Mensaje
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string MedioContacto { get; set; } = string.Empty;

    public string Asunto { get; set; } = string.Empty;

    /// <summary>
    /// Contenido del mensaje.
    /// </summary>
    public string Contenido { get; set; } = string.Empty;

    public DateTime FechaRecepcion { get; set; }

    public bool Atendido { get; set; }
}
