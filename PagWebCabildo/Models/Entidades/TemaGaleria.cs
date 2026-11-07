namespace PagWebCabildo.Models.Entidades;

/// <summary>
/// Tema de la galería que agrupa fotos y videos.
/// </summary>
public class TemaGaleria
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public string PortadaRuta { get; set; } = string.Empty;

    /// <summary>
    /// Elementos (fotos/videos) que pertenecen a este tema.
    /// </summary>
    public ICollection<ElementoGaleria> Elementos { get; set; } = new List<ElementoGaleria>();
}
