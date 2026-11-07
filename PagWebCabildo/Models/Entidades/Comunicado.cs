namespace PagWebCabildo.Models.Entidades;

/// <summary>
/// Comunicado publicado por el resguardo. Puede estar en borrador o publicado.
/// </summary>
public class Comunicado
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public DateTime Fecha { get; set; }

    public string Contenido { get; set; } = string.Empty;

    public EstadoComunicado Estado { get; set; }
}
