namespace PagWebCabildo.Models.Entidades;

/// <summary>
/// Documento descargable asociado a una <see cref="CategoriaDocumento"/>.
/// </summary>
public class Documento
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public string ArchivoRuta { get; set; } = string.Empty;

    public int CategoriaDocumentoId { get; set; }

    /// <summary>
    /// Categoría a la que pertenece este documento.
    /// </summary>
    public CategoriaDocumento? CategoriaDocumento { get; set; }
}
