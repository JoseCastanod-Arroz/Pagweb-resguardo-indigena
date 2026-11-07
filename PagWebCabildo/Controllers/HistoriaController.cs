using Microsoft.AspNetCore.Mvc;
using PagWebCabildo.Services;

namespace PagWebCabildo.Controllers;

/// <summary>
/// Controlador público de la sección de Historia y presentación del resguardo
/// (Requisito 3). Muestra el texto de la <c>PresentacionResguardo</c>.
/// </summary>
public class HistoriaController : Controller
{
    private readonly IPresentacionService _presentacionService;

    public HistoriaController(IPresentacionService presentacionService)
    {
        _presentacionService = presentacionService;
    }

    /// <summary>
    /// Muestra el texto de presentación/historia del resguardo (Requisito 3.1).
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var presentacion = await _presentacionService.ObtenerPresentacion();
        return View(presentacion);
    }
}
