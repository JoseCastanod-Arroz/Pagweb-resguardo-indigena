using System.ComponentModel.DataAnnotations;
using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Areas.Admin.Models;

/// <summary>
/// Modelo de vista del formulario de creación de cuentas de administrador
/// (Requisito 21.1). La contraseña es obligatoria al crear, ya que la nueva cuenta
/// necesita credenciales desde el inicio.
/// </summary>
public class UsuarioCrearViewModel
{
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
    /// Contraseña en texto plano a <i>hashear</i>. Obligatoria al crear.
    /// </summary>
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Contraseña { get; set; } = string.Empty;

    /// <summary>
    /// Rol asignado a la cuenta (Administrador general, Secretario, Tesorero,
    /// Fiscal o Comisario).
    /// </summary>
    [Display(Name = "Rol")]
    public Rol Rol { get; set; } = Rol.Secretario;
}
