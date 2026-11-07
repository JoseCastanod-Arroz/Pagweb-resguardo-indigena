using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PagWebCabildo.Models;
using PagWebCabildo.Models.ViewModels;
using PagWebCabildo.Services;

namespace PagWebCabildo.Controllers;

public class HomeController : Controller
{
    private const int MaximoElementosInicio = 4;

    private readonly IPresentacionService _presentacionService;
    private readonly IEventoService _eventoService;
    private readonly IComunicadoService _comunicadoService;

    public HomeController(
        IPresentacionService presentacionService,
        IEventoService eventoService,
        IComunicadoService comunicadoService)
    {
        _presentacionService = presentacionService;
        _eventoService = eventoService;
        _comunicadoService = comunicadoService;
    }

    public async Task<IActionResult> Index()
    {
        var presentacion = await _presentacionService.ObtenerPresentacion();
        var eventos = await _eventoService.ListarOrdenadosPorProximidad();
        var comunicados = await _comunicadoService.ListarPublicadosRecientesPrimero();

        var modelo = new InicioViewModel
        {
            Presentacion = presentacion.Texto,
            UltimosEventos = eventos.Take(MaximoElementosInicio).ToList(),
            UltimosComunicados = comunicados.Take(MaximoElementosInicio).ToList()
        };

        return View(modelo);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
