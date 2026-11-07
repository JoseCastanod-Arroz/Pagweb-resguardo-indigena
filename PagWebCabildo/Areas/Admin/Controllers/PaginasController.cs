using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PagWebCabildo.Areas.Admin.Models;
using PagWebCabildo.Services;

namespace PagWebCabildo.Areas.Admin.Controllers;

/// <summary>
/// Controlador de edición de páginas de contenido del Panel de Administración
/// (Area Admin). Permite a cualquier administrador autenticado editar el texto de
/// historia y presentación del resguardo (Requisito 24.1), que opera sobre la
/// entidad singleton <c>PresentacionResguardo</c> a través de
/// <see cref="IPresentacionService"/>.
/// </summary>
[Area("Admin")]
[Authorize(Policy = "GestionHistoria")]
public sealed class PaginasController : Controller
{
    private const string ModuloPresentacion = "Presentacion";

    private readonly IPresentacionService _presentacionService;
    private readonly IRegistroActividadService _registroActividad;

    public PaginasController(
        IPresentacionService presentacionService,
        IRegistroActividadService registroActividad)
    {
        _presentacionService = presentacionService;
        _registroActividad = registroActividad;
    }

    /// <summary>
    /// Muestra el formulario de edición del texto de historia/presentación con el
    /// contenido actual cargado (Requisito 24.1).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Historia()
    {
        var presentacion = await _presentacionService.ObtenerPresentacion();
        var modelo = new HistoriaViewModel { Texto = presentacion.Texto };
        return View(modelo);
    }

    /// <summary>
    /// Persiste el nuevo texto de historia/presentación sobre el registro singleton
    /// y vuelve a mostrar el formulario con un mensaje de éxito (Requisito 24.1).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Historia(HistoriaViewModel modelo)
    {
        var presentacion = await _presentacionService.ActualizarPresentacion(modelo.Texto ?? string.Empty);
        modelo.Texto = presentacion.Texto;

        await RegistrarActividad("Editar", "Texto de historia y presentación del resguardo");

        TempData["Mensaje"] = "El texto de historia y presentación se actualizó correctamente.";
        return View(modelo);
    }

    /// <summary>
    /// Anota una acción de contenido en el registro de actividad (veeduría) a nombre
    /// del administrador autenticado, tomando su identificador del claim
    /// <see cref="ClaimTypes.NameIdentifier"/> (Requisito 5.1). Si no se puede
    /// resolver un identificador válido, no registra nada. La escritura es resiliente
    /// por diseño del servicio y no interrumpe la operación principal.
    /// </summary>
    private async Task RegistrarActividad(string accion, string detalle)
    {
        if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var administradorId))
        {
            await _registroActividad.Registrar(administradorId, accion, ModuloPresentacion, detalle);
        }
    }
}
