namespace PagWebCabildo.Models.Entidades;

/// <summary>
/// Comentario anónimo a una <see cref="PublicacionForo"/>. Igual que las
/// publicaciones, se envía por un visitante y permanece pendiente de moderación
/// hasta que un administrador lo aprueba.
/// </summary>
public class ComentarioForo
{
    public int Id { get; set; }

    /// <summary>Nombre con el que firma el visitante (anónimo, texto libre).</summary>
    public string Autor { get; set; } = string.Empty;

    public string Contenido { get; set; } = string.Empty;

    /// <summary>Estado de moderación. Inicia en <see cref="EstadoPublicacionForo.Pendiente"/>.</summary>
    public EstadoPublicacionForo Estado { get; set; } = EstadoPublicacionForo.Pendiente;

    /// <summary>Fecha y hora de envío por parte del visitante.</summary>
    public DateTime FechaEnvio { get; set; }

    /// <summary>Publicación a la que pertenece el comentario (clave foránea).</summary>
    public int PublicacionForoId { get; set; }

    /// <summary>Navegación a la publicación propietaria.</summary>
    public PublicacionForo? PublicacionForo { get; set; }
}
