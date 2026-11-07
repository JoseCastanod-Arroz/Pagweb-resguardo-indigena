namespace PagWebCabildo.Models.Entidades;

/// <summary>
/// Estado de moderación de una publicación o comentario del foro comunitario.
/// El contenido lo envían visitantes anónimos y permanece oculto al público
/// hasta que un administrador lo aprueba.
/// </summary>
public enum EstadoPublicacionForo
{
    /// <summary>Enviado por un visitante y a la espera de revisión. No es público.</summary>
    Pendiente = 0,

    /// <summary>Revisado y aprobado por un administrador. Es visible al público.</summary>
    Aprobado = 1,

    /// <summary>Rechazado por un administrador. No es público.</summary>
    Rechazado = 2
}
