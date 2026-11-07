using System.ComponentModel.DataAnnotations;
using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Areas.Admin.Models;

/// <summary>
/// Modelo de vista para crear y editar un <see cref="Evento"/> desde el Panel de
/// Administración (Requisitos 26.1, 27.1, 28.1). Agrupa los campos editables del
/// evento y el archivo de imagen opcional subido por formulario. El guardado
/// físico de la imagen corre por cuenta de <c>IArchivoService</c> en el
/// controlador; la ruta relativa resultante se persiste en
/// <see cref="Evento.ImagenRuta"/>.
/// </summary>
public class EventoFormViewModel
{
    /// <summary>
    /// Identificador del evento. Es 0 al crear y el id existente al editar.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Título del evento (obligatorio; Requisito 26.1).
    /// </summary>
    [Display(Name = "Título")]
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200, MinimumLength = 1,
        ErrorMessage = "El título debe tener entre {2} y {1} caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    /// <summary>
    /// Fecha del evento (obligatoria; Requisito 26.1).
    /// </summary>
    [Display(Name = "Fecha")]
    [Required(ErrorMessage = "La fecha es obligatoria.")]
    [DataType(DataType.Date)]
    public DateOnly Fecha { get; set; }

    /// <summary>
    /// Hora del evento (obligatoria; Requisito 26.1).
    /// </summary>
    [Display(Name = "Hora")]
    [Required(ErrorMessage = "La hora es obligatoria.")]
    [DataType(DataType.Time)]
    public TimeOnly Hora { get; set; }

    /// <summary>
    /// Lugar del evento (Requisito 26.1).
    /// </summary>
    [Display(Name = "Lugar")]
    [StringLength(200)]
    public string Lugar { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del evento (Requisito 26.1).
    /// </summary>
    [Display(Name = "Descripción")]
    [StringLength(4000)]
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Ruta relativa de la imagen ya asociada al evento (solo al editar). Permite
    /// mostrar la imagen actual y conservarla si no se sube una nueva.
    /// </summary>
    public string? ImagenRutaActual { get; set; }

    /// <summary>
    /// Archivo de imagen subido por el formulario, opcional (Requisito 27.1). Si
    /// viene informado, el controlador lo guarda con <c>IArchivoService</c> y
    /// actualiza <see cref="Evento.ImagenRuta"/>.
    /// </summary>
    [Display(Name = "Imagen")]
    public IFormFile? Imagen { get; set; }
}
