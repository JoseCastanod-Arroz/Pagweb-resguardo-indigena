namespace PagWebCabildo.Models.Entidades;

/// <summary>
/// Texto de presentación/historia del resguardo. Entidad singleton (un único registro).
/// </summary>
public class PresentacionResguardo
{
    public int Id { get; set; }

    public string Texto { get; set; } = string.Empty;
}
