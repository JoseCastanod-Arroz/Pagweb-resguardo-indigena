using System.ComponentModel.DataAnnotations;
using PagWebCabildo.Models.Entidades;
using PagWebCabildo.Services;

namespace PagWebCabildo.Models.ViewModels;

/// <summary>
/// Modelo de vista compuesto para la sección pública de Contacto (Requisitos 15 y
/// 16). Agrupa la información de contacto del cabildo que se muestra al visitante
/// (<see cref="Informacion"/>) y los campos del formulario de contacto
/// (<see cref="Formulario"/>) que el visitante completa para enviar un mensaje.
/// </summary>
public class ContactoViewModel
{
    /// <summary>
    /// Información de contacto del cabildo mostrada en la sección (Requisito 15.1).
    /// </summary>
    public InformacionContacto Informacion { get; set; } = new InformacionContacto();

    /// <summary>
    /// Datos del formulario de contacto enviados por el visitante (Requisito 16).
    /// </summary>
    public ContactoFormViewModel Formulario { get; set; } = new ContactoFormViewModel();
}

/// <summary>
/// Campos del formulario de contacto con validación por Data Annotations
/// (Requisito 16.1). Los límites de longitud coinciden con las reglas del dominio
/// definidas en <see cref="IMensajeService"/>. La validez del medio de contacto se
/// comprueba además del lado del servidor con <see cref="IValidacionContactoService"/>
/// (Requisitos 16.2, 16.5).
/// </summary>
public class ContactoFormViewModel
{
    /// <summary>
    /// Nombre del visitante (obligatorio, 1–100 caracteres; Requisito 16.1).
    /// </summary>
    [Display(Name = "Nombre")]
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(IMensajeService.NombreMaxLongitud, MinimumLength = 1,
        ErrorMessage = "El nombre debe tener entre {2} y {1} caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Medio de contacto: correo o teléfono válido (obligatorio; Requisitos 16.1,
    /// 16.2, 16.5).
    /// </summary>
    [Display(Name = "Medio de contacto")]
    [Required(ErrorMessage = "El medio de contacto es obligatorio.")]
    public string MedioContacto { get; set; } = string.Empty;

    /// <summary>
    /// Asunto del mensaje (obligatorio, 1–150 caracteres; Requisito 16.1).
    /// </summary>
    [Display(Name = "Asunto")]
    [Required(ErrorMessage = "El asunto es obligatorio.")]
    [StringLength(IMensajeService.AsuntoMaxLongitud, MinimumLength = 1,
        ErrorMessage = "El asunto debe tener entre {2} y {1} caracteres.")]
    public string Asunto { get; set; } = string.Empty;

    /// <summary>
    /// Cuerpo del mensaje (obligatorio, 1–2000 caracteres; Requisito 16.1). Se
    /// mapea al campo <c>Contenido</c> de la entidad <see cref="Mensaje"/>.
    /// </summary>
    [Display(Name = "Mensaje")]
    [Required(ErrorMessage = "El mensaje es obligatorio.")]
    [StringLength(IMensajeService.ContenidoMaxLongitud, MinimumLength = 1,
        ErrorMessage = "El mensaje debe tener entre {2} y {1} caracteres.")]
    public string Mensaje { get; set; } = string.Empty;
}
