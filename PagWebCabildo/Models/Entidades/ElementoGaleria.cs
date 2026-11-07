namespace PagWebCabildo.Models.Entidades;

/// <summary>
/// Elemento (foto o video) que pertenece a un <see cref="TemaGaleria"/>.
/// </summary>
public class ElementoGaleria
{
    public int Id { get; set; }

    public TipoElemento Tipo { get; set; }

    public string ArchivoRuta { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public int TemaGaleriaId { get; set; }

    /// <summary>
    /// Tema de galería al que pertenece este elemento.
    /// </summary>
    public TemaGaleria? TemaGaleria { get; set; }
}
