using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PagWebCabildo.Models.Entidades;
using PagWebCabildo.Services;

namespace PagWebCabildo.Areas.Admin.Controllers;

/// <summary>
/// Controlador de veeduría (fiscalización) del Panel de Administración (Area Admin).
/// Expone únicamente la consulta de solo lectura del registro de actividad: el
/// listado de las acciones realizadas por los administradores sobre los módulos de
/// contenido, del más reciente al más antiguo (Requisitos 5.2, 5.3).
///
/// Aplica la política <c>Veeduria</c> a nivel de clase, que restringe el acceso al
/// Administrador General y al Fiscal. No ofrece acciones de creación, edición ni
/// eliminación: el registro de actividad se escribe desde los servicios de
/// contenido y aquí solo se consulta.
/// </summary>
[Area("Admin")]
[Authorize(Policy = "Veeduria")]
public sealed class RegistroActividadController : Controller
{
    private readonly IRegistroActividadService _registroActividadService;

    public RegistroActividadController(IRegistroActividadService registroActividadService)
    {
        _registroActividadService = registroActividadService;
    }

    /// <summary>
    /// Lista los registros de actividad ordenados del más reciente al más antiguo
    /// para su consulta de solo lectura (Requisitos 5.2, 5.3).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var registros = await _registroActividadService.Listar();
        return View(registros);
    }
}
