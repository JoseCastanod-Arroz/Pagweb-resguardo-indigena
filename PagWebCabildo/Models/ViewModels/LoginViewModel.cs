using System.ComponentModel.DataAnnotations;

namespace PagWebCabildo.Models.ViewModels;

/// <summary>
/// Modelo de vista del formulario de inicio de sesión del Panel de Administración
/// (Requisito 18.1). Recoge el nombre de usuario, la contraseña y, opcionalmente,
/// la URL de retorno a la que se redirige tras un inicio de sesión correcto.
/// </summary>
public class LoginViewModel
{
    /// <summary>
    /// Nombre de usuario de la cuenta de administrador.
    /// </summary>
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [Display(Name = "Usuario")]
    public string Usuario { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña en texto plano a verificar contra el hash almacenado.
    /// </summary>
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Contraseña { get; set; } = string.Empty;

    /// <summary>
    /// URL local a la que redirigir tras iniciar sesión. Se valida con
    /// <c>Url.IsLocalUrl</c> antes de usarse para evitar redirecciones abiertas.
    /// </summary>
    public string? ReturnUrl { get; set; }
}
