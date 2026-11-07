using System.ComponentModel.DataAnnotations;

namespace PagWebCabildo.Areas.Admin.Models;

/// <summary>
/// Modelo de vista del formulario de edición de la información de contacto del
/// resguardo en el Panel de Administración (Requisito 40.1). Recoge los datos que
/// se persisten sobre la entidad singleton <c>InformacionContacto</c>.
/// </summary>
public class ContactoAdminViewModel
{
    /// <summary>
    /// Dirección física del resguardo.
    /// </summary>
    [Display(Name = "Dirección")]
    public string Direccion { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono de contacto.
    /// </summary>
    [Display(Name = "Teléfono")]
    public string Telefono { get; set; } = string.Empty;

    /// <summary>
    /// Correo electrónico de contacto.
    /// </summary>
    [Display(Name = "Correo electrónico")]
    public string CorreoElectronico { get; set; } = string.Empty;

    /// <summary>
    /// Otros datos de contacto (redes, horarios, etc.).
    /// </summary>
    [Display(Name = "Otros datos")]
    public string OtrosDatos { get; set; } = string.Empty;
}
