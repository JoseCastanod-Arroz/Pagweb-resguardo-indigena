namespace PagWebCabildo.Models.Entidades;

/// <summary>
/// Acta de asamblea del cabildo. Documento privado, nunca accesible desde el sitio público.
/// </summary>
public class Acta
{
    public int Id { get; set; }

    /// <summary>
    /// Número consecutivo del acta.
    /// </summary>
    public int Consecutivo { get; set; }

    /// <summary>
    /// Fecha en que se realizó la asamblea.
    /// </summary>
    public DateOnly FechaAsamblea { get; set; }

    /// <summary>
    /// Tipo de asamblea: ordinaria o extraordinaria.
    /// </summary>
    public TipoAsamblea Tipo { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Ruta del archivo adjunto, almacenado en <c>uploads/actas</c>.
    /// </summary>
    public string ArchivoRuta { get; set; } = string.Empty;

    /// <summary>
    /// Administrador que registró el acta.
    /// </summary>
    public int RegistradoPorId { get; set; }

    /// <summary>
    /// Administrador que registró el acta.
    /// </summary>
    public Administrador? RegistradoPor { get; set; }

    /// <summary>
    /// Fecha y hora en que se registró el acta en el sistema.
    /// </summary>
    public DateTime FechaRegistro { get; set; }
}
