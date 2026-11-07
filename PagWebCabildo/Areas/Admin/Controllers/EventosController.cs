using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PagWebCabildo.Areas.Admin.Models;
using PagWebCabildo.Models.Entidades;
using PagWebCabildo.Services;

namespace PagWebCabildo.Areas.Admin.Controllers;

/// <summary>
/// Controlador de gestión de <see cref="Evento"/> del Panel de Administración
/// (Area Admin). Cubre el listado, la creación con imagen opcional, la edición,
/// el marcado de cancelación y la eliminación de eventos (Requisitos 26.1, 27.1,
/// 28.1, 28.2, 28.3 y 29.1). Es distinto del controlador público de Eventos: este
/// exige la política <c>GestionComunicadosEventos</c> (Administrador General,
/// Secretario y Comisario), por lo que roles como Tesorero o Fiscal reciben un
/// rechazo de autorización. La eliminación añade además la política
/// <c>EliminarContenido</c>, que la restringe al Administrador General. El guardado
/// físico de la imagen se delega en <see cref="IArchivoService"/>.
/// </summary>
[Area("Admin")]
[Authorize(Policy = "GestionComunicadosEventos")]
public sealed class EventosController : Controller
{
    private const string SubcarpetaEventos = "eventos";

    private const string ModuloEvento = "Evento";

    private readonly IEventoService _eventoService;
    private readonly IArchivoService _archivoService;
    private readonly IRegistroActividadService _registroActividad;

    public EventosController(
        IEventoService eventoService,
        IArchivoService archivoService,
        IRegistroActividadService registroActividad)
    {
        _eventoService = eventoService;
        _archivoService = archivoService;
        _registroActividad = registroActividad;
    }

    /// <summary>
    /// Lista los eventos ordenados por proximidad de fecha para su gestión
    /// (Requisito 26.1).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var eventos = await _eventoService.ListarOrdenadosPorProximidad();
        return View(eventos);
    }

    /// <summary>
    /// Muestra el formulario de creación de un evento (Requisito 26.1).
    /// </summary>
    [HttpGet]
    public IActionResult Crear()
    {
        return View(new EventoFormViewModel());
    }

    /// <summary>
    /// Crea un evento con sus datos y, si se adjunta, asocia la imagen subida
    /// mediante <see cref="IArchivoService"/> (Requisitos 26.1, 27.1).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(EventoFormViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        string? imagenRuta = null;
        if (modelo.Imagen is { Length: > 0 })
        {
            try
            {
                imagenRuta = await _archivoService.Guardar(modelo.Imagen, SubcarpetaEventos);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(nameof(modelo.Imagen), ex.Message);
                return View(modelo);
            }
        }

        var evento = new Evento
        {
            Titulo = modelo.Titulo,
            Fecha = modelo.Fecha,
            Hora = modelo.Hora,
            Lugar = modelo.Lugar,
            Descripcion = modelo.Descripcion,
            ImagenRuta = imagenRuta
        };

        await _eventoService.Crear(evento);
        await RegistrarActividad("Crear", $"Evento '{evento.Titulo}'");
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Muestra el formulario de edición de un evento existente (Requisito 28.1).
    /// Devuelve 404 si el evento no existe.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var evento = await _eventoService.Obtener(id);
        if (evento is null)
        {
            return NotFound();
        }

        var modelo = new EventoFormViewModel
        {
            Id = evento.Id,
            Titulo = evento.Titulo,
            Fecha = evento.Fecha,
            Hora = evento.Hora,
            Lugar = evento.Lugar,
            Descripcion = evento.Descripcion,
            ImagenRutaActual = evento.ImagenRuta
        };

        return View(modelo);
    }

    /// <summary>
    /// Guarda los cambios de un evento (Requisito 28.1). Si se sube una nueva
    /// imagen, la guarda con <see cref="IArchivoService"/>, actualiza la ruta y
    /// elimina el archivo anterior; si no se sube ninguna, conserva la imagen
    /// existente (Requisito 28.2).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(EventoFormViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var existente = await _eventoService.Obtener(modelo.Id);
        if (existente is null)
        {
            return NotFound();
        }

        var imagenRuta = existente.ImagenRuta;
        var rutaAnterior = existente.ImagenRuta;

        if (modelo.Imagen is { Length: > 0 })
        {
            try
            {
                imagenRuta = await _archivoService.Guardar(modelo.Imagen, SubcarpetaEventos);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(nameof(modelo.Imagen), ex.Message);
                modelo.ImagenRutaActual = existente.ImagenRuta;
                return View(modelo);
            }
        }

        var actualizado = new Evento
        {
            Id = existente.Id,
            Titulo = modelo.Titulo,
            Fecha = modelo.Fecha,
            Hora = modelo.Hora,
            Lugar = modelo.Lugar,
            Descripcion = modelo.Descripcion,
            ImagenRuta = imagenRuta,
            Cancelado = existente.Cancelado
        };

        await _eventoService.Editar(actualizado);

        // Si se reemplazó la imagen, elimina el archivo físico anterior.
        if (modelo.Imagen is { Length: > 0 } && !string.IsNullOrEmpty(rutaAnterior))
        {
            _archivoService.Eliminar(rutaAnterior);
        }

        await RegistrarActividad("Editar", $"Evento '{actualizado.Titulo}' (Id {actualizado.Id})");

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Marca un evento como cancelado (Requisito 29.1). El evento permanece
    /// visible y recuperable.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarCancelado(int id)
    {
        var evento = await _eventoService.MarcarCancelado(id);
        if (evento is null)
        {
            return NotFound();
        }

        // La cancelación es un cambio de estado del evento: se registra como edición
        // para la veeduría (Requisito 5.1).
        await RegistrarActividad("Editar", $"Evento cancelado '{evento.Titulo}' (Id {evento.Id})");

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Elimina un evento y su imagen física asociada (Requisito 28.2). La política
    /// <c>EliminarContenido</c> restringe la acción al Administrador General, por lo
    /// que un Editor recibe un rechazo de autorización (Requisito 28.3).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "EliminarContenido")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var evento = await _eventoService.Obtener(id);
        if (evento is null)
        {
            return NotFound();
        }

        var imagenRuta = evento.ImagenRuta;
        await _eventoService.Eliminar(id);

        if (!string.IsNullOrEmpty(imagenRuta))
        {
            _archivoService.Eliminar(imagenRuta);
        }

        await RegistrarActividad("Eliminar", $"Evento '{evento.Titulo}' (Id {id})");

        return RedirectToAction(nameof(Index));
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
            await _registroActividad.Registrar(administradorId, accion, ModuloEvento, detalle);
        }
    }
}
