namespace PagWebCabildo.Models.Entidades;

/// <summary>
/// Publicación del foro comunitario (receta o historia) enviada por un visitante
/// anónimo. El contenido queda pendiente de moderación y solo es público cuando
/// un administrador lo aprueba. No tiene relación con <see cref="Administrador"/>
/// porque el autor es anónimo: solo se guarda el nombre que la persona escribe.
/// </summary>
public class PublicacionForo
{
    public int Id { get; set; }

    /// <summary>Nombre con el que firma el visitante (anónimo, texto libre).</summary>
    public string Autor { get; set; } = string.Empty;

    /// <summary>Tipo de publicación: receta o historia.</summary>
    public TipoPublicacionForo Tipo { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Contenido { get; set; } = string.Empty;

    /// <summary>Ruta relativa de la imagen asociada. Opcional.</summary>
    public string? ImagenRuta { get; set; }

    /// <summary>Estado de moderación. Inicia en <see cref="EstadoPublicacionForo.Pendiente"/>.</summary>
    public EstadoPublicacionForo Estado { get; set; } = EstadoPublicacionForo.Pendiente;

    /// <summary>Fecha y hora de envío por parte del visitante.</summary>
    public DateTime FechaEnvio { get; set; }

    /// <summary>Comentarios enviados a esta publicación.</summary>
    public ICollection<ComentarioForo> Comentarios { get; set; } = new List<ComentarioForo>();
}
