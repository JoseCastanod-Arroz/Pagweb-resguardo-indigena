using System.ComponentModel.DataAnnotations;
using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Areas.Admin.Models;

/// <summary>
/// Modelo de vista para subir y editar un <see cref="Documento"/> desde el Panel
/// de Administración (Requisitos 38.1, 39.1). Agrupa los campos editables del
/// documento y el archivo subido por formulario. El guardado físico del archivo
/// corre por cuenta de <c>IArchivoService</c> en el controlador; la ruta relativa
/// resultante se persiste en <see cref="Documento.ArchivoRuta"/>.
/// </summary>
public class DocumentoFormViewModel
{
    /// <summary>
    /// Identificador del documento. Es 0 al crear y el id existente al editar.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Título del documento (obligatorio; Requisito 38.1).
    /// </summary>
    [Display(Name = "Título")]
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200, MinimumLength = 1,
        ErrorMessage = "El título debe tener entre {2} y {1} caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del documento (Requisito 38.1).
    /// </summary>
    [Display(Name = "Descripción")]
    [StringLength(4000)]
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Categoría a la que pertenece el documento (obligatoria; Requisito 38.1).
    /// </summary>
    [Display(Name = "Categoría")]
    [Required(ErrorMessage = "La categoría es obligatoria.")]
    public int CategoriaDocumentoId { get; set; }

    /// <summary>
    /// Ruta relativa del archivo ya asociado al documento (solo al editar).
    /// Permite conservar el archivo existente si no se sube uno nuevo.
    /// </summary>
    public string? ArchivoRutaActual { get; set; }

    /// <summary>
    /// Archivo subido por el formulario. Es obligatorio al subir un documento
    /// nuevo (Requisito 38.1) y opcional al editar, donde solo se reemplaza el
    /// archivo si viene informado (Requisito 39.2). La obligatoriedad al crear se
    /// valida en el controlador.
    /// </summary>
    [Display(Name = "Archivo")]
    public IFormFile? Archivo { get; set; }
}
