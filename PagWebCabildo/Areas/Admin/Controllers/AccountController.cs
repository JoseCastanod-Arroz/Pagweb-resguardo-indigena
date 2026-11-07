using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PagWebCabildo.Models.ViewModels;
using PagWebCabildo.Services;

namespace PagWebCabildo.Areas.Admin.Controllers;

/// <summary>
/// Controlador de autenticación del Panel de Administración (Area Admin). Gestiona
/// el inicio y el cierre de sesión mediante autenticación por cookies (configurada
/// en <c>Program.cs</c>, tarea 7.1). Es el destino de <c>LoginPath</c>, por lo que
/// las acciones de login son anónimas para evitar bucles de redirección.
/// </summary>
[Area("Admin")]
public sealed class AccountController : Controller
{
    private readonly IAutenticacionService _autenticacionService;

    public AccountController(IAutenticacionService autenticacionService)
    {
        _autenticacionService = autenticacionService;
    }

    /// <summary>
    /// Muestra el formulario de inicio de sesión (Requisito 18.1).
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        var modelo = new LoginViewModel { ReturnUrl = returnUrl };
        return View(modelo);
    }

    /// <summary>
    /// Procesa el inicio de sesión. Con credenciales válidas crea una sesión
    /// autenticada y redirige al panel principal o a una URL local de retorno
    /// (Requisitos 18.1, 19.1). Con credenciales inválidas o cuenta inactiva no
    /// crea ninguna sesión y vuelve a mostrar el formulario con un mensaje de error
    /// (Requisitos 18.2, 22.2).
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var administrador = await _autenticacionService
            .ValidarCredenciales(modelo.Usuario, modelo.Contraseña);

        if (administrador is null)
        {
            ModelState.AddModelError(string.Empty, "Credenciales incorrectas");
            return View(modelo);
        }

        // El claim de rol usa Rol.ToString() para coincidir con las políticas de
        // autorización definidas en Program.cs (nameof(Rol.AdministradorGeneral)).
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, administrador.Id.ToString()),
            new(ClaimTypes.Name, administrador.NombreUsuario),
            new(ClaimTypes.Role, administrador.Rol.ToString())
        };

        var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identidad);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        // Solo se redirige a URLs locales para evitar redirecciones abiertas.
        if (!string.IsNullOrEmpty(modelo.ReturnUrl) && Url.IsLocalUrl(modelo.ReturnUrl))
        {
            return Redirect(modelo.ReturnUrl);
        }

        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
    }

    /// <summary>
    /// Cierra la sesión del administrador autenticado (Requisito 19.1) y redirige
    /// de nuevo al inicio de sesión.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    /// <summary>
    /// Pantalla de acceso denegado. Es el destino de
    /// <c>AccessDeniedPath = "/Admin/Account/AccesoDenegado"</c> (configurado en
    /// <c>Program.cs</c>): cuando un administrador autenticado pero sin el rol
    /// exigido por una política (por ejemplo, un Editor que intenta gestionar
    /// cuentas) accede a un módulo restringido, se le redirige aquí y se le informa
    /// que la acción no está permitida para su rol (Requisitos 21.2, 22.3, 23.5).
    /// </summary>
    [HttpGet]
    public IActionResult AccesoDenegado()
    {
        return View();
    }
}
