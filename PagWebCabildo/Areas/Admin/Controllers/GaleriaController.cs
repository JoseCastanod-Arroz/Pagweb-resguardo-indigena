using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PagWebCabildo.Areas.Admin.Models;
using PagWebCabildo.Models.Entidades;
using PagWebCabildo.Services;

namespace PagWebCabildo.Areas.Admin.Controllers;

/// <summary>
/// Controlador de gestión de la galería (temas y elementos) del Panel de
/// Administración (Area Admin). Cubre la creación, edición y eliminación de temas
/// (<see cref="TemaGaleria"/>) y la carga, edición y eliminación de sus elementos
/// (<see cref="ElementoGaleria"/>: fotos y videos), con sus portadas y archivos
/// (Requisitos 33.1, 34.1, 34.2, 34.3, 35.1, 36.1, 36.2 y 36.3). Es distinto del
/// controlador público de Galería: este requiere autenticación y aplica la política
/// <c>EliminarContenido</c> para eliminar temas y elementos. El guardado físico de
/// portadas y archivos se delega en <see cref="IArchivoService"/>.
/// </summary>
[Area("Admin")]
[Authorize(Policy = "GestionContenidoEditorial")]
public sealed class GaleriaController : Controller
{
    private const string SubcarpetaPortadas = "galeria/portadas";
    private const string SubcarpetaElementos = "galeria/elementos";

    private const string ModuloGaleria = "Galeria";

    private readonly IGaleriaService _galeriaService;
    private readonly IArchivoService _archivoService;
    private readonly IRegistroActividadService _registroActividad;

    public GaleriaController(
        IGaleriaService galeriaService,
        IArchivoService archivoService,
        IRegistroActividadService registroActividad)
    {
        _galeriaService = galeriaService;
        _archivoService = archivoService;
        _registroActividad = registroActividad;
    }

    // --- Temas ---

    /// <summary>
    /// Lista los temas de la galería para su gestión (Requisito 33.1).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var temas = await _galeriaService.ListarTemas();
        return View(temas);
    }

    /// <summary>
    /// Muestra la página de detalle de un tema con sus elementos (fotos y videos)
    /// para gestionarlos (Requisitos 35.1, 36.1). Devuelve 404 si el tema no existe.
    /// </summary>
    [HttpGet]
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
    /// Muestra el formulario de creación de un tema (Requisito 33.1).
    /// </summary>
    [HttpGet]
    public IActionResult CrearTema()
    {
        return View(new TemaFormViewModel());
    }

    /// <summary>
    /// Crea un tema con sus datos y, si se adjunta, asocia la imagen de portada
    /// subida mediante <see cref="IArchivoService"/> (Requisito 33.1).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearTema(TemaFormViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var portadaRuta = string.Empty;
        if (modelo.Portada is { Length: > 0 })
        {
            try
            {
                portadaRuta = await _archivoService.Guardar(modelo.Portada, SubcarpetaPortadas);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(nameof(modelo.Portada), ex.Message);
                return View(modelo);
            }
        }

        var tema = new TemaGaleria
        {
            Nombre = modelo.Nombre,
            Descripcion = modelo.Descripcion,
            PortadaRuta = portadaRuta
        };

        await _galeriaService.CrearTema(tema);
        await RegistrarActividad("Crear", $"Tema de galería '{tema.Nombre}'");
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Muestra el formulario de edición de un tema existente (Requisito 34.1).
    /// Devuelve 404 si el tema no existe.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> EditarTema(int id)
    {
        var tema = await _galeriaService.ObtenerTema(id);
        if (tema is null)
        {
            return NotFound();
        }

        var modelo = new TemaFormViewModel
        {
            Id = tema.Id,
            Nombre = tema.Nombre,
            Descripcion = tema.Descripcion,
            PortadaRutaActual = tema.PortadaRuta
        };

        return View(modelo);
    }

    /// <summary>
    /// Guarda los cambios de un tema (Requisito 34.1). Si se sube una nueva portada,
    /// la guarda con <see cref="IArchivoService"/>, actualiza la ruta y elimina el
    /// archivo anterior; si no se sube ninguna, conserva la portada existente
    /// (Requisito 34.2).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarTema(TemaFormViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var existente = await _galeriaService.ObtenerTema(modelo.Id);
        if (existente is null)
        {
            return NotFound();
        }

        var portadaRuta = existente.PortadaRuta;
        var rutaAnterior = existente.PortadaRuta;

        if (modelo.Portada is { Length: > 0 })
        {
            try
            {
                portadaRuta = await _archivoService.Guardar(modelo.Portada, SubcarpetaPortadas);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(nameof(modelo.Portada), ex.Message);
                modelo.PortadaRutaActual = existente.PortadaRuta;
                return View(modelo);
            }
        }

        var actualizado = new TemaGaleria
        {
            Id = existente.Id,
            Nombre = modelo.Nombre,
            Descripcion = modelo.Descripcion,
            PortadaRuta = portadaRuta
        };

        await _galeriaService.EditarTema(actualizado);

        // Si se reemplazó la portada, elimina el archivo físico anterior.
        if (modelo.Portada is { Length: > 0 } && !string.IsNullOrEmpty(rutaAnterior))
        {
            _archivoService.Eliminar(rutaAnterior);
        }

        await RegistrarActividad("Editar", $"Tema de galería '{actualizado.Nombre}' (Id {actualizado.Id})");

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Elimina un tema y, en cascada, sus elementos (Requisito 34.2). También
    /// elimina los archivos físicos de la portada y de cada elemento. La política
    /// <c>EliminarContenido</c> restringe la acción al Administrador General, por lo
    /// que un Editor recibe un rechazo de autorización (Requisito 34.3).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "EliminarContenido")]
    public async Task<IActionResult> EliminarTema(int id)
    {
        var tema = await _galeriaService.ObtenerTema(id);
        if (tema is null)
        {
            return NotFound();
        }

        // Rutas de archivos a eliminar tras borrar el tema de la base de datos.
        var portadaRuta = tema.PortadaRuta;
        var rutasElementos = tema.Elementos
            .Select(e => e.ArchivoRuta)
            .Where(r => !string.IsNullOrEmpty(r))
            .ToList();

        await _galeriaService.EliminarTema(id);

        if (!string.IsNullOrEmpty(portadaRuta))
        {
            _archivoService.Eliminar(portadaRuta);
        }

        foreach (var ruta in rutasElementos)
        {
            _archivoService.Eliminar(ruta);
        }

        await RegistrarActividad("Eliminar", $"Tema de galería '{tema.Nombre}' (Id {id})");

        return RedirectToAction(nameof(Index));
    }

    // --- Elementos ---

    /// <summary>
    /// Muestra el formulario para subir un elemento (foto o video) a un tema
    /// (Requisito 35.1). Devuelve 404 si el tema no existe.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> SubirElemento(int temaId)
    {
        var tema = await _galeriaService.ObtenerTema(temaId);
        if (tema is null)
        {
            return NotFound();
        }

        var modelo = new ElementoFormViewModel
        {
            TemaGaleriaId = tema.Id,
            TemaNombre = tema.Nombre
        };

        return View(modelo);
    }

    /// <summary>
    /// Sube una foto o un video a un tema: guarda el archivo con
    /// <see cref="IArchivoService"/> y asocia el <see cref="ElementoGaleria"/> al
    /// tema mediante su clave foránea (Requisito 35.1).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubirElemento(ElementoFormViewModel modelo)
    {
        var tema = await _galeriaService.ObtenerTema(modelo.TemaGaleriaId);
        if (tema is null)
        {
            return NotFound();
        }

        if (modelo.Archivo is not { Length: > 0 })
        {
            ModelState.AddModelError(nameof(modelo.Archivo), "Debe seleccionar un archivo.");
        }

        if (!ModelState.IsValid)
        {
            modelo.TemaNombre = tema.Nombre;
            return View(modelo);
        }

        string archivoRuta;
        try
        {
            archivoRuta = await _archivoService.Guardar(modelo.Archivo!, SubcarpetaElementos);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(nameof(modelo.Archivo), ex.Message);
            modelo.TemaNombre = tema.Nombre;
            return View(modelo);
        }

        var elemento = new ElementoGaleria
        {
            TemaGaleriaId = modelo.TemaGaleriaId,
            Tipo = modelo.Tipo,
            ArchivoRuta = archivoRuta,
            Descripcion = modelo.Descripcion
        };

        await _galeriaService.AgregarElemento(elemento);
        await RegistrarActividad(
            "Crear",
            $"Elemento de galería ({elemento.Tipo}) en tema '{tema.Nombre}' (Id tema {elemento.TemaGaleriaId})");
        return RedirectToAction(nameof(Tema), new { id = modelo.TemaGaleriaId });
    }

    /// <summary>
    /// Muestra el formulario de edición de un elemento existente (Requisito 36.1).
    /// Devuelve 404 si el elemento no existe.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> EditarElemento(int id)
    {
        var elemento = await _galeriaService.ObtenerElemento(id);
        if (elemento is null)
        {
            return NotFound();
        }

        var modelo = new ElementoFormViewModel
        {
            Id = elemento.Id,
            TemaGaleriaId = elemento.TemaGaleriaId,
            Tipo = elemento.Tipo,
            Descripcion = elemento.Descripcion,
            ArchivoRutaActual = elemento.ArchivoRuta
        };

        return View(modelo);
    }

    /// <summary>
    /// Guarda los cambios de un elemento: descripción y tipo (Requisito 36.1). Si se
    /// sube un nuevo archivo, lo guarda con <see cref="IArchivoService"/>, actualiza
    /// la ruta y elimina el archivo anterior (Requisito 36.2); si no, conserva el
    /// archivo existente.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarElemento(ElementoFormViewModel modelo)
    {
        var existente = await _galeriaService.ObtenerElemento(modelo.Id);
        if (existente is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            modelo.ArchivoRutaActual = existente.ArchivoRuta;
            return View(modelo);
        }

        var archivoRuta = existente.ArchivoRuta;
        var rutaAnterior = existente.ArchivoRuta;

        if (modelo.Archivo is { Length: > 0 })
        {
            try
            {
                archivoRuta = await _archivoService.Guardar(modelo.Archivo, SubcarpetaElementos);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(nameof(modelo.Archivo), ex.Message);
                modelo.ArchivoRutaActual = existente.ArchivoRuta;
                return View(modelo);
            }
        }

        var actualizado = new ElementoGaleria
        {
            Id = existente.Id,
            TemaGaleriaId = existente.TemaGaleriaId,
            Tipo = modelo.Tipo,
            ArchivoRuta = archivoRuta,
            Descripcion = modelo.Descripcion
        };

        await _galeriaService.EditarElemento(actualizado);

        // Si se reemplazó el archivo, elimina el archivo físico anterior.
        if (modelo.Archivo is { Length: > 0 } && !string.IsNullOrEmpty(rutaAnterior))
        {
            _archivoService.Eliminar(rutaAnterior);
        }

        await RegistrarActividad(
            "Editar",
            $"Elemento de galería (Id {actualizado.Id}) del tema Id {actualizado.TemaGaleriaId}");

        return RedirectToAction(nameof(Tema), new { id = existente.TemaGaleriaId });
    }

    /// <summary>
    /// Elimina un elemento y su archivo físico asociado (Requisito 36.2). La política
    /// <c>EliminarContenido</c> restringe la acción al Administrador General, por lo
    /// que un Editor recibe un rechazo de autorización (Requisito 36.3).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "EliminarContenido")]
    public async Task<IActionResult> EliminarElemento(int id)
    {
        var elemento = await _galeriaService.ObtenerElemento(id);
        if (elemento is null)
        {
            return NotFound();
        }

        var temaId = elemento.TemaGaleriaId;
        var archivoRuta = elemento.ArchivoRuta;

        await _galeriaService.EliminarElemento(id);

        if (!string.IsNullOrEmpty(archivoRuta))
        {
            _archivoService.Eliminar(archivoRuta);
        }

        await RegistrarActividad("Eliminar", $"Elemento de galería (Id {id}) del tema Id {temaId}");

        return RedirectToAction(nameof(Tema), new { id = temaId });
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
            await _registroActividad.Registrar(administradorId, accion, ModuloGaleria, detalle);
        }
    }
}
