using System.ComponentModel.DataAnnotations;
using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Areas.Admin.Models;

/// <summary>
/// Modelo de vista para crear y editar un <see cref="Acta"/> de asamblea desde el
/// Panel de Administración (Requisitos 3.1, 4.1, 4.2). Agrupa los campos editables
/// del acta y el archivo adjunto subido por formulario. El guardado físico del
/// archivo corre por cuenta de <c>IArchivoService</c> (vía <c>IActaService</c>) en
/// el controlador; la ruta relativa resultante se persiste en
/// <see cref="Acta.ArchivoRuta"/>. Las reglas de dominio (consecutivo positivo y
/// único, fecha obligatoria, tipo válido, título obligatorio) las impone
/// <c>IActaService</c>, que lanza <see cref="System.ArgumentException"/> con un
/// mensaje en español.
/// </summary>
public class ActaFormViewModel
{
    /// <summary>
    /// Identificador del acta. Es 0 al crear y el id existente al editar.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Número consecutivo del acta (obligatorio y positivo; Requisito 3.2).
    /// </summary>
    [Display(Name = "Consecutivo")]
    [Range(1, int.MaxValue, ErrorMessage = "El consecutivo debe ser un número positivo.")]
    public int Consecutivo { get; set; }

    /// <summary>
    /// Fecha en que se realizó la asamblea (obligatoria; Requisito 3.2).
    /// </summary>
    [Display(Name = "Fecha de la asamblea")]
    [DataType(DataType.Date)]
    [Required(ErrorMessage = "La fecha de la asamblea es obligatoria.")]
    public DateOnly FechaAsamblea { get; set; }

    /// <summary>
    /// Tipo de asamblea: ordinaria o extraordinaria (Requisito 3.2).
    /// </summary>
    [Display(Name = "Tipo de asamblea")]
    public TipoAsamblea Tipo { get; set; }

    /// <summary>
    /// Título del acta (obligatorio; Requisito 3.2).
    /// </summary>
    [Display(Name = "Título")]
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200, MinimumLength = 1,
        ErrorMessage = "El título debe tener entre {2} y {1} caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del acta (Requisito 3.1).
    /// </summary>
    [Display(Name = "Descripción")]
    [StringLength(4000)]
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Ruta relativa del archivo ya asociado al acta (solo al editar). Permite
    /// conservar el archivo existente si no se sube uno nuevo.
    /// </summary>
    public string? ArchivoRutaActual { get; set; }

    /// <summary>
    /// Archivo adjunto subido por el formulario. Es obligatorio al crear un acta
    /// nueva (Requisito 4.1) y opcional al editar, donde solo se reemplaza el
    /// archivo si viene informado (Requisito 4.2). La obligatoriedad al crear se
    /// valida en el controlador.
    /// </summary>
    [Display(Name = "Archivo del acta")]
    public IFormFile? Archivo { get; set; }
}
