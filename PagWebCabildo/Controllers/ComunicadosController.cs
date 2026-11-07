using Microsoft.AspNetCore.Mvc;
using PagWebCabildo.Services;

namespace PagWebCabildo.Controllers;

/// <summary>
/// Controlador público de comunicados. Expone únicamente los comunicados
/// publicados; los borradores no son accesibles ni por listado ni por enlace
/// directo (Requisitos 8.1, 8.2, 9.1, 32.3).
/// </summary>
public class ComunicadosController : Controller
{
    private readonly IComunicadoService _comunicados;

    public ComunicadosController(IComunicadoService comunicados)
    {
        _comunicados = comunicados;
    }

    /// <summary>
    /// Lista los comunicados publicados ordenados del más reciente al más antiguo
    /// (Requisitos 8.1, 8.2).
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var comunicados = await _comunicados.ListarPublicadosRecientesPrimero();
        return View(comunicados);
    }

    /// <summary>
    /// Muestra el detalle de un comunicado publicado. Devuelve 404 si el
    /// comunicado no existe o está en borrador, impidiendo el acceso directo a
    /// borradores (Requisitos 9.1, 32.3).
    /// </summary>
    public async Task<IActionResult> Detalle(int id)
    {
        var comunicado = await _comunicados.ObtenerPublicado(id);
        if (comunicado is null)
        {
            return NotFound();
        }

        return View(comunicado);
    }
}
