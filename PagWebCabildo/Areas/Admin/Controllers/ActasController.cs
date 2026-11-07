using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PagWebCabildo.Areas.Admin.Models;
using PagWebCabildo.Models.Entidades;
using PagWebCabildo.Services;

namespace PagWebCabildo.Areas.Admin.Controllers;

/// <summary>
/// Controlador de gestión de <see cref="Acta"/> de asamblea del Panel de
/// Administración (Area Admin). Cubre el listado, la creación, la edición y la
/// descarga del archivo adjunto (Requisitos 3.1, 4.1, 4.2, 4.4). Las actas son
/// documentos privados que nunca se exponen en el sitio público: este módulo solo
/// vive en el Area Admin y exige autenticación.
///
/// La autorización se aplica por acción, no a nivel de clase, porque la consulta
/// (descarga) es más amplia que la gestión: <c>GestionActas</c>
/// (AdministradorGeneral, Secretario) protege <see cref="Index"/>, <see cref="Crear"/>
/// y <see cref="Editar"/>; <c>ConsultaActas</c> (AdministradorGeneral, Secretario,
/// Fiscal) protege <see cref="Descargar"/>, de modo que el Fiscal puede consultar y
/// descargar actas pero no gestionarlas.
///
/// Las reglas de dominio (consecutivo positivo y único, fecha obligatoria, tipo
/// válido, título obligatorio y extensión del adjunto) viven en
/// <see cref="IActaService"/>; aquí se traducen sus <see cref="ArgumentException"/>
/// a errores de formulario y sus resultados nulos a 404.
/// </summary>
[Area("Admin")]
[Authorize]
public sealed class ActasController : Controller
{
    private readonly IActaService _actaService;

    public ActasController(IActaService actaService)
    {
        _actaService = actaService;
    }

    /// <summary>
    /// Lista todas las actas de la más reciente a la más antigua para su gestión
    /// (Requisito 3.1).
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "GestionActas")]
    public async Task<IActionResult> Index()
    {
        var actas = await _actaService.Listar();
        return View(actas);
    }

    /// <summary>
    /// Muestra el formulario de creación de un acta (Requisitos 3.1, 4.1).
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "GestionActas")]
    public IActionResult Crear()
    {
        return View(new ActaFormViewModel());
    }

    /// <summary>
    /// Crea y registra un acta nueva con su archivo adjunto (Requisitos 3.1, 4.1).
    /// El archivo es obligatorio al crear. Fija <see cref="Acta.RegistradoPorId"/>
    /// con el identificador del administrador autenticado (claim
    /// <see cref="ClaimTypes.NameIdentifier"/>). Las validaciones de dominio y de
    /// extensión del adjunto las impone <see cref="IActaService.Crear"/>, que lanza
    /// <see cref="ArgumentException"/>; en ese caso se conserva lo enviado y se
    /// muestra el error en el formulario. En caso de éxito redirige al listado.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "GestionActas")]
    public async Task<IActionResult> Crear(ActaFormViewModel modelo)
    {
        if (modelo.Archivo is not { Length: > 0 })
        {
            ModelState.AddModelError(nameof(modelo.Archivo), "El archivo del acta es obligatorio.");
        }

        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var acta = new Acta
        {
            Consecutivo = modelo.Consecutivo,
            FechaAsamblea = modelo.FechaAsamblea,
            Tipo = modelo.Tipo,
            Titulo = modelo.Titulo,
            Descripcion = modelo.Descripcion,
            RegistradoPorId = ObtenerAdministradorId()
        };

        try
        {
            await _actaService.Crear(acta, modelo.Archivo!);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }

        TempData["Mensaje"] = "El acta se registró correctamente.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Muestra el formulario de edición de un acta existente (Requisito 4.2).
    /// Devuelve 404 si el acta no existe. El archivo es opcional al editar: si no
    /// se sube uno nuevo, se conserva el adjunto actual.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "GestionActas")]
    public async Task<IActionResult> Editar(int id)
    {
        var acta = await _actaService.Obtener(id);
        if (acta is null)
        {
            return NotFound();
        }

        var modelo = new ActaFormViewModel
        {
            Id = acta.Id,
            Consecutivo = acta.Consecutivo,
            FechaAsamblea = acta.FechaAsamblea,
            Tipo = acta.Tipo,
            Titulo = acta.Titulo,
            Descripcion = acta.Descripcion,
            ArchivoRutaActual = acta.ArchivoRuta
        };

        return View(modelo);
    }

    /// <summary>
    /// Guarda los cambios de un acta (Requisito 4.2). El archivo es opcional: si se
    /// sube uno nuevo se reemplaza el adjunto; si no, se conserva el existente. Las
    /// validaciones de dominio y de extensión las impone
    /// <see cref="IActaService.Editar"/>, que lanza <see cref="ArgumentException"/>;
    /// en ese caso se conserva lo enviado y se muestra el error. Devuelve 404 si el
    /// acta no existe.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "GestionActas")]
    public async Task<IActionResult> Editar(ActaFormViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var acta = new Acta
        {
            Id = modelo.Id,
            Consecutivo = modelo.Consecutivo,
            FechaAsamblea = modelo.FechaAsamblea,
            Tipo = modelo.Tipo,
            Titulo = modelo.Titulo,
            Descripcion = modelo.Descripcion
        };

        var archivo = modelo.Archivo is { Length: > 0 } ? modelo.Archivo : null;

        Acta? actualizada;
        try
        {
            actualizada = await _actaService.Editar(acta, archivo);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }

        if (actualizada is null)
        {
            return NotFound();
        }

        TempData["Mensaje"] = "El acta se actualizó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Descarga el archivo adjunto de un acta (Requisito 4.4). La política
    /// <c>ConsultaActas</c> permite esta acción al Administrador General, al
    /// Secretario y al Fiscal. Devuelve 404 si el acta no existe, no tiene archivo
    /// o el archivo físico no se encuentra.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "ConsultaActas")]
    public async Task<IActionResult> Descargar(int id)
    {
        ArchivoAbierto? archivoAbierto;
        try
        {
            archivoAbierto = await _actaService.AbrirArchivo(id);
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }

        if (archivoAbierto is null)
        {
            return NotFound();
        }

        return File(archivoAbierto.Stream, archivoAbierto.ContentType, archivoAbierto.Nombre);
    }

    /// <summary>
    /// Obtiene el identificador del administrador autenticado a partir del claim
    /// <see cref="ClaimTypes.NameIdentifier"/> que fija <c>AccountController</c> al
    /// iniciar sesión.
    /// </summary>
    private int ObtenerAdministradorId()
    {
        var valor = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(valor!);
    }
}
