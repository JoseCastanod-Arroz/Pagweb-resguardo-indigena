using System.ComponentModel.DataAnnotations;
using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Areas.Admin.Models;

/// <summary>
/// Modelo de vista para subir y editar un <see cref="ElementoGaleria"/> (foto o
/// video) de un tema desde el Panel de Administración (Requisitos 35.1, 36.1).
/// Agrupa el tipo de elemento, la descripción y el archivo subido por formulario. El
/// guardado físico del archivo corre por cuenta de <c>IArchivoService</c> en el
/// controlador; la ruta relativa resultante se persiste en
/// <see cref="ElementoGaleria.ArchivoRuta"/>.
/// </summary>
public class ElementoFormViewModel
{
    /// <summary>
    /// Identificador del elemento. Es 0 al subir y el id existente al editar.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Identificador del tema al que pertenece el elemento (Requisito 35.1).
    /// </summary>
    public int TemaGaleriaId { get; set; }

    /// <summary>
    /// Nombre del tema, solo para mostrar en el encabezado del formulario.
    /// </summary>
    public string? TemaNombre { get; set; }

    /// <summary>
    /// Tipo del elemento: foto o video (Requisito 35.1).
    /// </summary>
    [Display(Name = "Tipo")]
    [Required(ErrorMessage = "El tipo es obligatorio.")]
    public TipoElemento Tipo { get; set; }

    /// <summary>
    /// Descripción del elemento (Requisito 36.1).
    /// </summary>
    [Display(Name = "Descripción")]
    [StringLength(4000)]
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Ruta relativa del archivo ya asociado al elemento (solo al editar). Permite
    /// previsualizar el archivo actual y conservarlo si no se sube uno nuevo.
    /// </summary>
    public string? ArchivoRutaActual { get; set; }

    /// <summary>
    /// Archivo (foto o video) subido por el formulario. Obligatorio al subir un
    /// nuevo elemento; opcional al editar (Requisito 35.1).
    /// </summary>
    [Display(Name = "Archivo")]
    public IFormFile? Archivo { get; set; }
}
