using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PagWebCabildo.Models.Entidades;
using PagWebCabildo.Services;

namespace PagWebCabildo.Areas.Admin.Controllers;

/// <summary>
/// Controlador de gestión de <see cref="Mensaje"/> del Panel de Administración
/// (Area Admin). Cubre el listado del más reciente al más antiguo, el detalle de un
/// mensaje, el marcado como atendido y la eliminación (Requisitos 41.1, 41.2, 42.1,
/// 42.2, 43.1 y 43.2). Exige la política <c>GestionMensajes</c> (Administrador
/// General y Comisario), por lo que roles como Secretario, Tesorero o Fiscal reciben
/// un rechazo de autorización. La eliminación añade además la política
/// <c>EliminarContenido</c>, que la restringe al Administrador General (Requisito
/// 43.2).
/// </summary>
[Area("Admin")]
[Authorize(Policy = "GestionMensajes")]
public sealed class MensajesController : Controller
{
    private const string ModuloMensaje = "Mensaje";

    private readonly IMensajeService _mensajeService;
    private readonly IRegistroActividadService _registroActividad;

    public MensajesController(
        IMensajeService mensajeService,
        IRegistroActividadService registroActividad)
    {
        _mensajeService = mensajeService;
        _registroActividad = registroActividad;
    }

    /// <summary>
    /// Lista todos los mensajes ordenados del más reciente al más antiguo para su
    /// gestión (Requisitos 41.1, 41.2).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var mensajes = await _mensajeService.ListarRecientesPrimero();
        return View(mensajes);
    }

    /// <summary>
    /// Muestra el detalle de un mensaje (Requisito 42.1). Devuelve 404 si el mensaje
    /// no existe.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Ver(int id)
    {
        var mensaje = await _mensajeService.Ver(id);
        if (mensaje is null)
        {
            return NotFound();
        }

        return View(mensaje);
    }

    /// <summary>
    /// Marca un mensaje como atendido y regresa a su detalle (Requisito 42.2).
    /// Devuelve 404 si el mensaje no existe.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarAtendido(int id)
    {
        var mensaje = await _mensajeService.MarcarAtendido(id);
        if (mensaje is null)
        {
            return NotFound();
        }

        // Marcar como atendido es un cambio de estado del mensaje: se registra como
        // edición para la veeduría (Requisito 5.1).
        await RegistrarActividad("Editar", $"Mensaje marcado como atendido (Id {id})");

        return RedirectToAction(nameof(Ver), new { id });
    }

    /// <summary>
    /// Elimina un mensaje (Requisito 43.1). La política <c>EliminarContenido</c>
    /// restringe la acción al Administrador General, por lo que un Editor recibe un
    /// rechazo de autorización (Requisito 43.2).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "EliminarContenido")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _mensajeService.Eliminar(id);
        await RegistrarActividad("Eliminar", $"Mensaje (Id {id})");
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
            await _registroActividad.Registrar(administradorId, accion, ModuloMensaje, detalle);
        }
    }
}
