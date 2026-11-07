using Microsoft.AspNetCore.Mvc;
using PagWebCabildo.Models.Entidades;
using PagWebCabildo.Models.ViewModels;
using PagWebCabildo.Services;

namespace PagWebCabildo.Controllers;

/// <summary>
/// Controlador público del foro comunitario. Cualquier visitante anónimo puede
/// ver las publicaciones aprobadas (recetas e historias), enviar nuevas
/// publicaciones y comentar. Todo lo enviado queda pendiente de moderación y solo
/// se hace público cuando un administrador lo aprueba desde el Panel. Las imágenes
/// se guardan con <see cref="IArchivoService"/> bajo <c>wwwroot/uploads/foro</c>.
/// </summary>
public class ForoController : Controller
{
    private const string SubcarpetaForo = "foro";

    private readonly IForoService _foro;
    private readonly IArchivoService _archivos;

    public ForoController(IForoService foro, IArchivoService archivos)
    {
        _foro = foro;
        _archivos = archivos;
    }

    /// <summary>Número de publicaciones por página en el feed público.</summary>
    private const int TamañoPagina = 10;

    /// <summary>
    /// Feed público del foro (solo publicaciones aprobadas). Admite búsqueda por
    /// texto (<paramref name="q"/>), filtro por tipo (<paramref name="tipo"/>),
    /// ordenamiento (<paramref name="orden"/>) y paginación (<paramref name="pagina"/>).
    /// Todos los parámetros se devuelven al modelo para conservar el estado en los
    /// controles de la vista.
    /// </summary>
    public async Task<IActionResult> Index(
        string? q,
        TipoPublicacionForo? tipo,
        OrdenForo orden = OrdenForo.Recientes,
        int pagina = 1)
    {
        var feed = await _foro.ListarAprobadas(q, tipo, orden, pagina, TamañoPagina);
        return View(feed);
    }

    /// <summary>
    /// Muestra el detalle de una publicación aprobada con sus comentarios
    /// aprobados y el formulario para comentar. Devuelve 404 si no existe o no
    /// está aprobada (impide el acceso directo a contenido pendiente/rechazado).
    /// </summary>
    public async Task<IActionResult> Detalle(int id)
    {
        var publicacion = await _foro.ObtenerAprobadaConComentarios(id);
        if (publicacion is null)
        {
            return NotFound();
        }

        var modelo = new DetallePublicacionForoViewModel
        {
            Publicacion = publicacion,
            NuevoComentario = new ComentarioForoFormViewModel { PublicacionForoId = id }
        };
        return View(modelo);
    }

    /// <summary>
    /// Muestra el formulario para que un visitante envíe una receta o historia.
    /// </summary>
    [HttpGet]
    public IActionResult Crear()
    {
        return View(new PublicacionForoFormViewModel());
    }

    /// <summary>
    /// Procesa el envío de una publicación anónima. Guarda la imagen opcional y
    /// registra la publicación en estado pendiente. Usa el patrón PRG.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(PublicacionForoFormViewModel modelo)
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
                imagenRuta = await _archivos.Guardar(modelo.Imagen, SubcarpetaForo);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(nameof(modelo.Imagen), ex.Message);
                return View(modelo);
            }
        }

        await _foro.CrearPublicacion(new PublicacionForo
        {
            Autor = modelo.Autor,
            Tipo = modelo.Tipo,
            Titulo = modelo.Titulo,
            Contenido = modelo.Contenido,
            ImagenRuta = imagenRuta
        });

        TempData["MensajeExito"] =
            "¡Gracias por tu aporte! Tu publicación fue enviada y será visible cuando un administrador la apruebe.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Procesa el envío de un comentario anónimo a una publicación aprobada. El
    /// comentario queda pendiente de moderación. Usa el patrón PRG.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Comentar(
        [Bind(Prefix = "NuevoComentario")] ComentarioForoFormViewModel nuevoComentario)
    {
        if (!ModelState.IsValid)
        {
            // Recarga el detalle conservando los errores del formulario.
            var publicacion = await _foro.ObtenerAprobadaConComentarios(nuevoComentario.PublicacionForoId);
            if (publicacion is null)
            {
                return NotFound();
            }

            var vm = new DetallePublicacionForoViewModel
            {
                Publicacion = publicacion,
                NuevoComentario = nuevoComentario
            };
            return View(nameof(Detalle), vm);
        }

        var creado = await _foro.CrearComentario(new ComentarioForo
        {
            PublicacionForoId = nuevoComentario.PublicacionForoId,
            Autor = nuevoComentario.Autor,
            Contenido = nuevoComentario.Contenido
        });

        if (creado is null)
        {
            return NotFound();
        }

        TempData["MensajeExito"] =
            "¡Gracias por tu comentario! Será visible cuando un administrador lo apruebe.";
        return RedirectToAction(nameof(Detalle), new { id = nuevoComentario.PublicacionForoId });
    }
}
