using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PagWebCabildo.Models.Entidades;
using PagWebCabildo.Services;

namespace PagWebCabildo.Areas.Admin.Controllers;

/// <summary>
/// Controlador de gestión de autoridades, cargos y comités del Panel de
/// Administración (Area Admin, Requisito 25). Cualquier administrador autenticado
/// puede crear, editar y reordenar autoridades; únicamente un
/// <c>AdministradorGeneral</c> puede eliminarlas, lo que se garantiza con la
/// política de autorización <c>"EliminarContenido"</c> sobre la acción
/// <see cref="Eliminar"/> (Requisito 25.5).
/// </summary>
[Area("Admin")]
[Authorize(Policy = "GestionContenidoEditorial")]
public sealed class AutoridadesController : Controller
{
    private const string ModuloAutoridad = "Autoridad";

    private readonly IAutoridadService _autoridadService;
    private readonly IRegistroActividadService _registroActividad;

    public AutoridadesController(
        IAutoridadService autoridadService,
        IRegistroActividadService registroActividad)
    {
        _autoridadService = autoridadService;
        _registroActividad = registroActividad;
    }

    /// <summary>
    /// Lista las autoridades en el orden definido (ascendente por <c>Orden</c>),
    /// que es la fuente de verdad del orden de presentación (Requisito 25.3).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var autoridades = await _autoridadService.Listar();
        return View(autoridades);
    }

    /// <summary>
    /// Muestra el formulario de creación de una autoridad (Requisito 25.1).
    /// </summary>
    [HttpGet]
    public IActionResult Crear()
    {
        return View(new Autoridad());
    }

    /// <summary>
    /// Registra una nueva autoridad con nombre, cargo, tipo y orden
    /// (Requisito 25.1).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(Autoridad modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        await _autoridadService.Crear(modelo);
        await RegistrarActividad("Crear", $"Autoridad '{modelo.Nombre}'");
        TempData["Mensaje"] = "Autoridad creada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Muestra el formulario de edición de una autoridad existente
    /// (Requisito 25.2).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var autoridad = await _autoridadService.Obtener(id);
        if (autoridad is null)
        {
            return NotFound();
        }

        return View(autoridad);
    }

    /// <summary>
    /// Guarda los cambios de una autoridad existente (Requisito 25.2).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, Autoridad modelo)
    {
        if (id != modelo.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var existente = await _autoridadService.Obtener(id);
        if (existente is null)
        {
            return NotFound();
        }

        await _autoridadService.Editar(modelo);
        await RegistrarActividad("Editar", $"Autoridad '{modelo.Nombre}' (Id {modelo.Id})");
        TempData["Mensaje"] = "Autoridad actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Aplica el nuevo orden de las autoridades (Requisito 25.3 y 25.4). Recibe
    /// arreglos paralelos de identificadores y valores de orden enviados desde el
    /// formulario del listado y los convierte en los pares <c>(id, orden)</c> que
    /// espera <see cref="IAutoridadService.ReordenarAutoridades"/>.
    /// </summary>
    /// <param name="ids">Identificadores de las autoridades a reordenar.</param>
    /// <param name="ordenes">Nuevo valor de <c>Orden</c> para cada identificador,
    /// en el mismo orden posicional que <paramref name="ids"/>.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ordenar(int[] ids, int[] ordenes)
    {
        if (ids is null || ordenes is null || ids.Length != ordenes.Length)
        {
            TempData["Error"] = "No se pudo aplicar el orden: los datos recibidos no son válidos.";
            return RedirectToAction(nameof(Index));
        }

        var pares = ids
            .Zip(ordenes, (id, orden) => (id, orden))
            .ToList();

        await _autoridadService.ReordenarAutoridades(pares);
        TempData["Mensaje"] = "Orden de las autoridades guardado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Elimina una autoridad. Restringida a <c>AdministradorGeneral</c> mediante la
    /// política <c>"EliminarContenido"</c>; un Editor es rechazado y redirigido a la
    /// pantalla de acceso denegado (Requisito 25.5).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "EliminarContenido")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _autoridadService.Eliminar(id);
        await RegistrarActividad("Eliminar", $"Autoridad (Id {id})");
        TempData["Mensaje"] = "Autoridad eliminada correctamente.";
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
            await _registroActividad.Registrar(administradorId, accion, ModuloAutoridad, detalle);
        }
    }
}
