using Microsoft.AspNetCore.Mvc;
using PagWebCabildo.Services;

namespace PagWebCabildo.Controllers;

/// <summary>
/// Controlador público de la sección de Organización del resguardo (Requisito 4).
/// Muestra las autoridades, cargos y comités en el orden definido.
/// </summary>
public class OrganizacionController : Controller
{
    private readonly IAutoridadService _autoridadService;

    public OrganizacionController(IAutoridadService autoridadService)
    {
        _autoridadService = autoridadService;
    }

    /// <summary>
    /// Muestra las autoridades, cargos y comités ordenados ascendentemente por
    /// <c>Orden</c> (Requisitos 4.1 y 4.2).
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var autoridades = await _autoridadService.Listar();
        return View(autoridades);
    }
}
