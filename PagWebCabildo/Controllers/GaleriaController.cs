using Microsoft.AspNetCore.Mvc;
using PagWebCabildo.Services;

namespace PagWebCabildo.Controllers;

/// <summary>
/// Controlador público de la Galería. Muestra los temas con su portada, las
/// fotos y videos de cada tema, y la foto ampliada con su descripción
/// (Requisitos 10.1, 10.2, 11.1, 12.1). No pertenece a ningún Area: vive en el
/// espacio de nombres raíz de controladores del sitio público.
/// </summary>
public class GaleriaController : Controller
{
    private readonly IGaleriaService _galeriaService;

    public GaleriaController(IGaleriaService galeriaService)
    {
        _galeriaService = galeriaService;
    }

    /// <summary>
    /// Lista los temas de la galería, cada uno con su nombre, descripción y
    /// portada, enlazando al detalle de cada tema (Requisitos 10.1, 10.2).
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var temas = await _galeriaService.ListarTemas();
        return View(temas);
    }

    /// <summary>
    /// Muestra las fotos y videos de un tema (Requisito 11.1). Devuelve 404 si el
    /// tema no existe.
    /// </summary>
    public async Task<IActionResult> Tema(int id)
    {
        var tema = await _galeriaService.ObtenerTema(id);
        if (tema is null)
        {
            return NotFound();
        }

        return View(tema);
    }

    /// <summary>
    /// Muestra una foto ampliada con su descripción (Requisito 12.1). Devuelve 404
    /// si el elemento no existe.
    /// </summary>
    public async Task<IActionResult> Foto(int id)
    {
        var elemento = await _galeriaService.ObtenerElemento(id);
        if (elemento is null)
        {
            return NotFound();
        }

        return View(elemento);
    }
}
