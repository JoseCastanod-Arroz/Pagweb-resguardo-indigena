using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PagWebCabildo.Areas.Admin.Controllers;

/// <summary>
/// Controlador del panel principal del Panel de Administración (Area Admin).
/// Es la página de inicio tras autenticarse y ofrece accesos directos a cada
/// módulo de gestión (Requisito 20.1). Requiere únicamente que el administrador
/// esté autenticado; las restricciones por rol se aplican en cada módulo.
/// </summary>
[Area("Admin")]
[Authorize]
public sealed class DashboardController : Controller
{
    /// <summary>
    /// Muestra el panel principal con accesos a cada módulo de gestión
    /// (Requisito 20.1).
    /// </summary>
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}
