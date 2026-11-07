using System.ComponentModel.DataAnnotations;
using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Areas.Admin.Models;

/// <summary>
/// Modelo de vista del formulario de edición de cuentas de administrador
/// (Requisito 22.1). La contraseña es opcional: si se deja vacía se conserva la
/// contraseña actual; si se suministra, se vuelve a <i>hashear</i>.
/// </summary>
public class UsuarioEditarViewModel
{
    /// <summary>
    /// Identificador de la cuenta que se edita.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nombre de la persona titular de la cuenta.
    /// </summary>
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Nombre de usuario único con el que la cuenta inicia sesión.
    /// </summary>
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre de usuario no puede superar los 100 caracteres.")]
    [Display(Name = "Nombre de usuario")]
    public string NombreUsuario { get; set; } = string.Empty;

    /// <summary>
    /// Nueva contraseña en texto plano, o vacía para conservar la actual.
    /// </summary>
    [DataType(DataType.Password)]
    [Display(Name = "Nueva contraseña (opcional)")]
    public string? Contraseña { get; set; }

    /// <summary>
    /// Rol asignado a la cuenta (Administrador general, Secretario, Tesorero,
    /// Fiscal o Comisario).
    /// </summary>
    [Display(Name = "Rol")]
    public Rol Rol { get; set; }
}
