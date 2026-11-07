using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PagWebCabildo.Models.Entidades;
using PagWebCabildo.Services;

namespace PagWebCabildo.Areas.Admin.Controllers;

/// <summary>
/// Controlador de moderación del foro comunitario (Area Admin). Permite revisar
/// las publicaciones (recetas e historias) y los comentarios que envían los
/// visitantes anónimos, aprobarlos o rechazarlos, y eliminarlos. El acceso se
/// restringe con la política <c>ModerarForo</c> (Administrador General y
/// Comisario); la eliminación exige además <c>EliminarContenido</c> (solo el
/// Administrador General). El borrado físico de las imágenes se delega en
/// <see cref="IArchivoService"/>.
/// </summary>
[Area("Admin")]
[Authorize(Policy = "ModerarForo")]
public sealed class ForoController : Controller
{
    private const string ModuloPublicacion = "PublicacionForo";
    private const string ModuloComentario = "ComentarioForo";

    private readonly IForoService _foro;
    private readonly IArchivoService _archivos;
    private readonly IRegistroActividadService _registroActividad;

    public ForoController(
        IForoService foro,
        IArchivoService archivos,
        IRegistroActividadService registroActividad)
    {
        _foro = foro;
        _archivos = archivos;
        _registroActividad = registroActividad;
    }

    /// <summary>
    /// Lista todas las publicaciones (cualquier estado) para su moderación.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var publicaciones = await _foro.ListarTodasPublicaciones();
        return View(publicaciones);
    }

    /// <summary>
    /// Muestra el detalle de una publicación con todos sus comentarios para
    /// moderarlos individualmente. Devuelve 404 si no existe.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var publicacion = await _foro.ObtenerPublicacionConComentarios(id);
        if (publicacion is null)
        {
            return NotFound();
        }

        return View(publicacion);
    }

    // ----- Moderación de publicaciones -----

    /// <summary>Aprueba una publicación (la hace pública). Devuelve 404 si no existe.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AprobarPublicacion(int id)
    {
        var publicacion = await _foro.CambiarEstadoPublicacion(id, EstadoPublicacionForo.Aprobado);
        if (publicacion is null)
        {
            return NotFound();
        }

        await RegistrarActividad("Aprobar", ModuloPublicacion, $"Publicación '{publicacion.Titulo}' (Id {id})");
        TempData["Mensaje"] = "Publicación aprobada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Rechaza una publicación (la oculta). Devuelve 404 si no existe.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RechazarPublicacion(int id)
    {
        var publicacion = await _foro.CambiarEstadoPublicacion(id, EstadoPublicacionForo.Rechazado);
        if (publicacion is null)
        {
            return NotFound();
        }

        await RegistrarActividad("Rechazar", ModuloPublicacion, $"Publicación '{publicacion.Titulo}' (Id {id})");
        TempData["Mensaje"] = "Publicación rechazada.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Elimina una publicación, sus comentarios (en cascada) y su imagen física.
    /// Restringida al Administrador General por la política <c>EliminarContenido</c>.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "EliminarContenido")]
    public async Task<IActionResult> EliminarPublicacion(int id)
    {
        var publicacion = await _foro.ObtenerPublicacion(id);
        if (publicacion is null)
        {
            return NotFound();
        }

        var imagenRuta = publicacion.ImagenRuta;
        var eliminada = await _foro.EliminarPublicacion(id);
        if (!eliminada)
        {
            return NotFound();
        }

        if (!string.IsNullOrEmpty(imagenRuta))
        {
            _archivos.Eliminar(imagenRuta);
        }

        await RegistrarActividad("Eliminar", ModuloPublicacion, $"Publicación '{publicacion.Titulo}' (Id {id})");
        TempData["Mensaje"] = "Publicación eliminada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // ----- Moderación de comentarios -----

    /// <summary>Aprueba un comentario (lo hace público). Devuelve 404 si no existe.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AprobarComentario(int id, int publicacionId)
    {
        var comentario = await _foro.CambiarEstadoComentario(id, EstadoPublicacionForo.Aprobado);
        if (comentario is null)
        {
            return NotFound();
        }

        await RegistrarActividad("Aprobar", ModuloComentario, $"Comentario de '{comentario.Autor}' (Id {id})");
        TempData["Mensaje"] = "Comentario aprobado correctamente.";
        return RedirectToAction(nameof(Detalle), new { id = publicacionId });
    }

    /// <summary>Rechaza un comentario (lo oculta). Devuelve 404 si no existe.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RechazarComentario(int id, int publicacionId)
    {
        var comentario = await _foro.CambiarEstadoComentario(id, EstadoPublicacionForo.Rechazado);
        if (comentario is null)
        {
            return NotFound();
        }

        await RegistrarActividad("Rechazar", ModuloComentario, $"Comentario de '{comentario.Autor}' (Id {id})");
        TempData["Mensaje"] = "Comentario rechazado.";
        return RedirectToAction(nameof(Detalle), new { id = publicacionId });
    }

    /// <summary>
    /// Elimina un comentario. Restringida al Administrador General por la política
    /// <c>EliminarContenido</c>.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "EliminarContenido")]
    public async Task<IActionResult> EliminarComentario(int id, int publicacionId)
    {
        var eliminado = await _foro.EliminarComentario(id);
        if (!eliminado)
        {
            return NotFound();
        }

        await RegistrarActividad("Eliminar", ModuloComentario, $"Comentario (Id {id})");
        TempData["Mensaje"] = "Comentario eliminado correctamente.";
        return RedirectToAction(nameof(Detalle), new { id = publicacionId });
    }

    /// <summary>
    /// Anota una acción de moderación en el registro de actividad (veeduría) a
    /// nombre del administrador autenticado, tomando su identificador del claim
    /// <see cref="ClaimTypes.NameIdentifier"/>. Si no se puede resolver un
    /// identificador válido, no registra nada. La escritura es resiliente por
    /// diseño del servicio y no interrumpe la operación principal.
    /// </summary>
    private async Task RegistrarActividad(string accion, string modulo, string detalle)
    {
        if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var administradorId))
        {
            await _registroActividad.Registrar(administradorId, accion, modulo, detalle);
        }
    }
}
