using System.ComponentModel.DataAnnotations;
using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Areas.Admin.Models;

/// <summary>
/// Modelo de vista para crear y editar un <see cref="TemaGaleria"/> desde el Panel
/// de Administración (Requisitos 33.1, 34.1). Agrupa los campos editables del tema
/// y el archivo de portada opcional subido por formulario. El guardado físico de la
/// portada corre por cuenta de <c>IArchivoService</c> en el controlador; la ruta
/// relativa resultante se persiste en <see cref="TemaGaleria.PortadaRuta"/>.
/// </summary>
public class TemaFormViewModel
{
    /// <summary>
    /// Identificador del tema. Es 0 al crear y el id existente al editar.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nombre del tema (obligatorio; Requisito 33.1).
    /// </summary>
    [Display(Name = "Nombre")]
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(200, MinimumLength = 1,
        ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del tema (Requisito 33.1).
    /// </summary>
    [Display(Name = "Descripción")]
    [StringLength(4000)]
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Ruta relativa de la portada ya asociada al tema (solo al editar). Permite
    /// mostrar la portada actual y conservarla si no se sube una nueva.
    /// </summary>
    public string? PortadaRutaActual { get; set; }

    /// <summary>
    /// Archivo de imagen de portada subido por el formulario, opcional (Requisito
    /// 33.1). Si viene informado, el controlador lo guarda con <c>IArchivoService</c>
    /// y actualiza <see cref="TemaGaleria.PortadaRuta"/>.
    /// </summary>
    [Display(Name = "Portada")]
    public IFormFile? Portada { get; set; }
}
