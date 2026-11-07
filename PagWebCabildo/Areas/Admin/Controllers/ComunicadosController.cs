using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PagWebCabildo.Areas.Admin.Models;
using PagWebCabildo.Models.Entidades;
using PagWebCabildo.Services;

namespace PagWebCabildo.Areas.Admin.Controllers;

/// <summary>
/// Controlador de gestión de <see cref="Comunicado"/> del Panel de Administración
/// (Area Admin). Cubre el listado de todos los comunicados, la creación y edición,
/// el guardado como borrador con validación, la publicación (solo desde borrador) y
/// la eliminación (Requisitos 30.1, 31.1, 31.2, 31.3, 32.1, 32.2, 32.4, 32.5, 32.6).
///
/// Es distinto del controlador público de Comunicados: este exige la política
/// <c>GestionComunicadosEventos</c> (Administrador General, Secretario y Comisario),
/// por lo que roles como Tesorero o Fiscal reciben un rechazo de autorización. La
/// eliminación añade además la política <c>EliminarContenido</c>, que la restringe al
/// Administrador General (Requisito 31.3). Las reglas de dominio (borrador exige
/// título y contenido; publicar solo procede desde borrador) viven en
/// <see cref="IComunicadoService"/>; aquí se traducen sus excepciones a mensajes de
/// formulario o avisos <c>TempData</c>.
/// </summary>
[Area("Admin")]
[Authorize(Policy = "GestionComunicadosEventos")]
public sealed class ComunicadosController : Controller
{
    private const string ModuloComunicado = "Comunicado";

    private readonly IComunicadoService _comunicadoService;
    private readonly IRegistroActividadService _registroActividad;

    public ComunicadosController(
        IComunicadoService comunicadoService,
        IRegistroActividadService registroActividad)
    {
        _comunicadoService = comunicadoService;
        _registroActividad = registroActividad;
    }

    /// <summary>
    /// Lista todos los comunicados (borradores y publicados) para su gestión,
    /// mostrando título, fecha y estado (Requisito 30.1).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var comunicados = await _comunicadoService.ListarTodos();
        return View(comunicados);
    }

    /// <summary>
    /// Muestra el formulario de creación de un comunicado (Requisito 31.1). El
    /// guardado se encauza por <see cref="GuardarBorrador"/> o <see cref="Publicar"/>
    /// según el botón que envíe el formulario.
    /// </summary>
    [HttpGet]
    public IActionResult Crear()
    {
        return View(new ComunicadoFormViewModel());
    }

    /// <summary>
    /// Muestra el formulario de edición de un comunicado existente, permitiendo
    /// modificar título, fecha y contenido (Requisito 31.2). Devuelve 404 si no
    /// existe.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var comunicado = await _comunicadoService.Obtener(id);
        if (comunicado is null)
        {
            return NotFound();
        }

        var modelo = new ComunicadoFormViewModel
        {
            Id = comunicado.Id,
            Titulo = comunicado.Titulo,
            Fecha = comunicado.Fecha,
            Contenido = comunicado.Contenido
        };

        // El estado decide si la vista ofrece el botón Publicar (solo borradores).
        ViewBag.Estado = comunicado.Estado;
        return View(modelo);
    }

    /// <summary>
    /// Guarda un comunicado como borrador (Requisito 32.1). Si <c>modelo.Id</c> es 0
    /// crea uno nuevo; en caso contrario actualiza el existente. La regla de título y
    /// contenido obligatorios la impone <see cref="IComunicadoService.GuardarBorrador"/>,
    /// que lanza <see cref="ArgumentException"/>; en ese caso se vuelve a mostrar el
    /// formulario con el error y los datos enviados (Requisito 32.2). En caso de éxito
    /// redirige al listado.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuardarBorrador(ComunicadoFormViewModel modelo)
    {
        int? id = modelo.Id == 0 ? null : modelo.Id;
        var vistaFormulario = id is null ? "Crear" : "Editar";

        if (!ModelState.IsValid)
        {
            await PrepararEstadoEdicion(id);
            return View(vistaFormulario, modelo);
        }

        try
        {
            await _comunicadoService.GuardarBorrador(id, modelo.Titulo, modelo.Fecha, modelo.Contenido);
        }
        catch (ArgumentException ex)
        {
            // El servidor rechazó el borrador (título o contenido en blanco):
            // se conserva lo enviado y se muestra el error (Requisito 32.2).
            ModelState.AddModelError(string.Empty, ex.Message);
            await PrepararEstadoEdicion(id);
            return View(vistaFormulario, modelo);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }

        // Veeduría: un Id nulo corresponde a una creación; en otro caso, a una edición
        // (Requisito 5.1).
        var accion = id is null ? "Crear" : "Editar";
        await RegistrarActividad(accion, $"Comunicado '{modelo.Titulo}'");

        TempData["Mensaje"] = "El comunicado se guardó como borrador.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Publica un comunicado (Requisito 32.4). La operación solo procede desde el
    /// estado borrador; si el comunicado está en otro estado,
    /// <see cref="IComunicadoService.Publicar"/> lanza
    /// <see cref="InvalidOperationException"/> y aquí se surface un aviso
    /// <c>TempData</c> de acción no permitida sin cambiar el estado (Requisitos 32.5,
    /// 32.6). En caso de éxito redirige al listado.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publicar(int id)
    {
        try
        {
            await _comunicadoService.Publicar(id);
        }
        catch (InvalidOperationException)
        {
            TempData["Error"] = "No permitido: un comunicado solo puede publicarse desde el estado borrador.";
            return RedirectToAction(nameof(Index));
        }

        await RegistrarActividad("Publicar", $"Comunicado (Id {id})");

        TempData["Mensaje"] = "El comunicado se publicó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Elimina un comunicado (Requisito 31.3). La política <c>EliminarContenido</c>
    /// restringe la acción al Administrador General, por lo que un Editor recibe un
    /// rechazo de autorización.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "EliminarContenido")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var eliminado = await _comunicadoService.Eliminar(id);
        if (!eliminado)
        {
            return NotFound();
        }

        await RegistrarActividad("Eliminar", $"Comunicado (Id {id})");

        TempData["Mensaje"] = "El comunicado se eliminó.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Expone en <c>ViewBag.Estado</c> el estado del comunicado (o <c>null</c> si es
    /// nuevo) para que la vista de edición decida si ofrecer el botón Publicar al
    /// re-renderizar el formulario tras un error de validación.
    /// </summary>
    private async Task PrepararEstadoEdicion(int? id)
    {
        if (id is null)
        {
            ViewBag.Estado = null;
            return;
        }

        var comunicado = await _comunicadoService.Obtener(id.Value);
        ViewBag.Estado = comunicado?.Estado;
    }

    /// <summary>
    /// Anota una acción de contenido en el registro de actividad (veeduría) a nombre
    /// del administrador autenticado, tomando su identificador del claim
    /// <see cref="ClaimTypes.NameIdentifier"/> que fija el inicio de sesión
    /// (Requisito 5.1). Si no se puede resolver un identificador válido, no registra
    /// nada. La escritura es resiliente por diseño del servicio y no interrumpe la
    /// operación principal.
    /// </summary>
    private async Task RegistrarActividad(string accion, string detalle)
    {
        if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var administradorId))
        {
            await _registroActividad.Registrar(administradorId, accion, ModuloComunicado, detalle);
        }
    }
}
