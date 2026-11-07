using System.ComponentModel.DataAnnotations;
using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Areas.Admin.Models;

/// <summary>
/// Modelo de vista para crear y editar un <see cref="Comunicado"/> desde el Panel
/// de Administración (Requisitos 31.1, 31.2, 32.1, 32.2). Agrupa los campos
/// editables del comunicado. El guardado como borrador exige título y contenido;
/// esa regla la impone <c>IComunicadoService.GuardarBorrador</c> en el servidor y
/// se refuerza aquí con Data Annotations para la validación del formulario.
/// </summary>
public class ComunicadoFormViewModel
{
    /// <summary>
    /// Identificador del comunicado. Es 0 al crear y el id existente al editar.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Título del comunicado (obligatorio; Requisito 32.2).
    /// </summary>
    [Display(Name = "Título")]
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200, MinimumLength = 1,
        ErrorMessage = "El título debe tener entre {2} y {1} caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    /// <summary>
    /// Fecha del comunicado (Requisito 31.1).
    /// </summary>
    [Display(Name = "Fecha")]
    [Required(ErrorMessage = "La fecha es obligatoria.")]
    [DataType(DataType.Date)]
    public DateTime Fecha { get; set; } = DateTime.Today;

    /// <summary>
    /// Contenido del comunicado (obligatorio; Requisito 32.2).
    /// </summary>
    [Display(Name = "Contenido")]
    [Required(ErrorMessage = "El contenido es obligatorio.")]
    [StringLength(10000, MinimumLength = 1,
        ErrorMessage = "El contenido debe tener entre {2} y {1} caracteres.")]
    public string Contenido { get; set; } = string.Empty;
}
