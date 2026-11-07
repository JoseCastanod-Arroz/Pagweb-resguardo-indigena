using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PagWebCabildo.Areas.Admin.Models;
using PagWebCabildo.Services;

namespace PagWebCabildo.Areas.Admin.Controllers;

/// <summary>
/// Controlador de gestión de cuentas de administrador del Panel de Administración
/// (Requisitos 21, 22 y 23). Todo el controlador exige la política
/// <c>GestionCuentas</c>, que requiere el rol <c>AdministradorGeneral</c>; un Editor
/// que intente acceder es rechazado a nivel de política y redirigido a la pantalla
/// de acceso denegado (<c>AccessDeniedPath = "/Admin/Account/AccesoDenegado"</c>,
/// configurada en <c>Program.cs</c>), donde se muestra que la acción no está
/// permitida para su rol (Requisitos 21.2, 22.3, 23.5).
/// </summary>
[Area("Admin")]
[Authorize(Policy = "GestionCuentas")]
public sealed class UsuariosController : Controller
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    /// <summary>
    /// Lista todas las cuentas de administrador registradas.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var administradores = await _usuarioService.Listar();
        return View(administradores);
    }

    /// <summary>
    /// Muestra el formulario de creación de una cuenta de administrador
    /// (Requisito 21.1).
    /// </summary>
    [HttpGet]
    public IActionResult Crear()
    {
        return View(new UsuarioCrearViewModel());
    }

    /// <summary>
    /// Procesa la creación de una cuenta de administrador (Requisito 21.1). Si el
    /// nombre de usuario ya existe, se agrega un error al <see cref="ModelState"/>
    /// y se vuelve a mostrar el formulario con los datos ingresados.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(UsuarioCrearViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        try
        {
            await _usuarioService.Crear(
                modelo.Nombre,
                modelo.NombreUsuario,
                modelo.Contraseña,
                modelo.Rol);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(modelo.NombreUsuario), ex.Message);
            return View(modelo);
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Muestra el formulario de edición de una cuenta de administrador
    /// (Requisito 22.1).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var administrador = await _usuarioService.Obtener(id);
        if (administrador is null)
        {
            return NotFound();
        }

        var modelo = new UsuarioEditarViewModel
        {
            Id = administrador.Id,
            Nombre = administrador.Nombre,
            NombreUsuario = administrador.NombreUsuario,
            Rol = administrador.Rol
        };

        return View(modelo);
    }

    /// <summary>
    /// Procesa la edición de una cuenta de administrador (Requisito 22.1). La
    /// contraseña es opcional: si se deja vacía se conserva la actual. Si el nombre
    /// de usuario queda en uso por otra cuenta, se agrega un error al
    /// <see cref="ModelState"/> y se vuelve a mostrar el formulario.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, UsuarioEditarViewModel modelo)
    {
        if (id != modelo.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        try
        {
            await _usuarioService.Editar(
                modelo.Id,
                modelo.Nombre,
                modelo.NombreUsuario,
                modelo.Rol,
                modelo.Contraseña);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(modelo.NombreUsuario), ex.Message);
            return View(modelo);
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Desactiva una cuenta de administrador, impidiendo su inicio de sesión
    /// (Requisito 22.2).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desactivar(int id)
    {
        await _usuarioService.Desactivar(id);
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Elimina una cuenta de administrador (Requisito 23.1).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _usuarioService.Eliminar(id);
        return RedirectToAction(nameof(Index));
    }
}
