namespace PagWebCabildo.Models.Entidades;

/// <summary>
/// Evento de la comunidad.
/// </summary>
public class Evento
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public DateOnly Fecha { get; set; }

    public TimeOnly Hora { get; set; }

    public string Lugar { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Ruta relativa de la imagen asociada. Opcional.
    /// </summary>
    public string? ImagenRuta { get; set; }

    public bool Cancelado { get; set; }
}
