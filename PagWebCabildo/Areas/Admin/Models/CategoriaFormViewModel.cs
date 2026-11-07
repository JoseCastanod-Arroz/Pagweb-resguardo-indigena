using System.ComponentModel.DataAnnotations;
using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Areas.Admin.Models;

/// <summary>
/// Modelo de vista para crear y editar una <see cref="CategoriaDocumento"/> desde
/// el Panel de Administración (Requisitos 37.1, 37.2). Agrupa el único campo
/// editable de la categoría, su nombre.
/// </summary>
public class CategoriaFormViewModel
{
    /// <summary>
    /// Identificador de la categoría. Es 0 al crear y el id existente al editar.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nombre de la categoría de documentos (obligatorio; Requisito 37.1).
    /// </summary>
    [Display(Name = "Nombre")]
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(200, MinimumLength = 1,
        ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Indica si la categoría agrupa documentos de carácter financiero
    /// (Requisito 6.1). Los documentos de categorías financieras son gestionados
    /// por el rol Tesorero.
    /// </summary>
    [Display(Name = "Categoría financiera")]
    public bool EsFinanciero { get; set; }

    /// <summary>
    /// Indica si la categoría es pública (visible y descargable en el sitio
    /// público). Si es privada, solo es accesible desde el Panel de Administración.
    /// Por defecto, las categorías nuevas son públicas.
    /// </summary>
    [Display(Name = "Categoría pública (visible en el sitio)")]
    public bool EsPublica { get; set; } = true;
}
