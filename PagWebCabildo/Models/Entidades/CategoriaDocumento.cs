namespace PagWebCabildo.Models.Entidades;

/// <summary>
/// Categoría que agrupa documentos descargables.
/// </summary>
public class CategoriaDocumento
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Indica si la categoría agrupa documentos de carácter financiero.
    /// Los documentos de categorías financieras son gestionados por el rol Tesorero.
    /// </summary>
    public bool EsFinanciero { get; set; }

    /// <summary>
    /// Indica si la categoría es pública. Las categorías públicas (valor por
    /// defecto) se muestran y se pueden descargar en el sitio público; las
    /// privadas solo son accesibles desde el Panel de Administración y sus archivos
    /// no se sirven como estáticos.
    /// </summary>
    public bool EsPublica { get; set; } = true;

    /// <summary>
    /// Documentos que pertenecen a esta categoría.
    /// </summary>
    public ICollection<Documento> Documentos { get; set; } = new List<Documento>();
}
