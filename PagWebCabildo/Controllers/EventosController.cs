using Microsoft.AspNetCore.Mvc;
using PagWebCabildo.Services;

namespace PagWebCabildo.Controllers;

/// <summary>
/// Controlador público de Eventos. Muestra el listado de eventos ordenados por
/// proximidad de fecha y el detalle de un evento (Requisitos 5.1, 5.2, 6.1, 6.2,
/// 7.1). No pertenece a ningún Area: vive en el espacio de nombres raíz de
/// controladores del sitio público.
/// </summary>
public class EventosController : Controller
{
    private readonly IEventoService _eventoService;

    public EventosController(IEventoService eventoService)
    {
        _eventoService = eventoService;
    }

    /// <summary>
    /// Lista los eventos ordenados por proximidad de fecha (Requisitos 5.1, 5.2).
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var eventos = await _eventoService.ListarOrdenadosPorProximidad();
        return View(eventos);
    }

    /// <summary>
    /// Muestra el detalle de un evento: título, fecha, hora, lugar, descripción,
    /// imagen opcional e indicador de cancelado (Requisitos 6.1, 6.2, 7.1).
    /// Devuelve 404 si el evento no existe.
    /// </summary>
    public async Task<IActionResult> Detalle(int id)
    {
        var evento = await _eventoService.Obtener(id);
        if (evento is null)
        {
            return NotFound();
        }

        return View(evento);
    }
}
