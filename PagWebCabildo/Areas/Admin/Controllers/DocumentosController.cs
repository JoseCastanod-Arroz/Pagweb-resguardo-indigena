using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PagWebCabildo.Areas.Admin.Models;
using PagWebCabildo.Models.Entidades;
using PagWebCabildo.Services;

namespace PagWebCabildo.Areas.Admin.Controllers;

/// <summary>
/// Controlador de gestión de <see cref="CategoriaDocumento"/> y
/// <see cref="Documento"/> del Panel de Administración (Area Admin). Cubre la
/// administración de categorías (listado, creación, edición y eliminación) y de
/// documentos (carga con archivo, edición con reemplazo de archivo y eliminación)
/// (Requisitos 37.1, 37.2, 37.3, 37.4, 38.1, 39.1, 39.2 y 39.3). Es distinto del
/// controlador público de Documentos: este requiere autenticación y aplica la
/// política <c>EliminarContenido</c> para eliminar categorías y documentos. El
/// guardado y la eliminación física de los archivos se delegan en
/// <see cref="IArchivoService"/>.
/// </summary>
[Area("Admin")]
[Authorize(Policy = "GestionDocumentos")]
public sealed class DocumentosController : Controller
{
    private const string SubcarpetaDocumentos = "documentos";
    private const string SubcarpetaDocumentosPrivados = "documentos-privados";
    private const string ModuloDocumento = "Documento";
    private const string ModuloCategoriaDocumento = "CategoriaDocumento";

    private readonly IDocumentoService _documentoService;
    private readonly IArchivoService _archivoService;
    private readonly IRegistroActividadService _registroActividad;

    public DocumentosController(
        IDocumentoService documentoService,
        IArchivoService archivoService,
        IRegistroActividadService registroActividad)
    {
        _documentoService = documentoService;
        _archivoService = archivoService;
        _registroActividad = registroActividad;
    }

    // ----- Landing de gestión -----

    /// <summary>
    /// Muestra el listado de documentos agrupados por categoría, con accesos a la
    /// gestión de categorías y documentos (Requisito 37.1).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var categorias = await _documentoService.ListarAgrupadosPorCategoria();
        return View(categorias);
    }

    /// <summary>
    /// Muestra los documentos de categorías financieras para el rol Tesorero
    /// (Requisitos 6.1, 6.2, 6.3). La política <c>GestionFinanciera</c> restringe
    /// el acceso al Administrador General y al Tesorero, de modo que este rol
    /// dispone de una ruta autorizada hacia los documentos financieros sin obtener
    /// acceso al resto de la gestión documental.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "GestionFinanciera")]
    public async Task<IActionResult> Financieros()
    {
        var documentos = await _documentoService.ListarDocumentosFinancieros();
        return View(documentos);
    }

    // ----- Gestión de categorías -----

    /// <summary>
    /// Lista las categorías de documentos para su gestión (Requisito 37.1).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Categorias()
    {
        var categorias = await _documentoService.ListarCategorias();
        return View(categorias);
    }

    /// <summary>
    /// Muestra el formulario de creación de una categoría (Requisito 37.1).
    /// </summary>
    [HttpGet]
    public IActionResult CrearCategoria()
    {
        return View(new CategoriaFormViewModel());
    }

    /// <summary>
    /// Crea y registra una nueva categoría de documentos (Requisito 37.1).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearCategoria(CategoriaFormViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        await _documentoService.CrearCategoria(
            new CategoriaDocumento
            {
                Nombre = modelo.Nombre,
                EsFinanciero = modelo.EsFinanciero,
                EsPublica = modelo.EsPublica
            });
        await RegistrarActividad("Crear", ModuloCategoriaDocumento, $"Categoría de documentos '{modelo.Nombre}'");
        TempData["Mensaje"] = "Categoría creada correctamente.";
        return RedirectToAction(nameof(Categorias));
    }

    /// <summary>
    /// Muestra el formulario de edición de una categoría existente
    /// (Requisito 37.2). Devuelve 404 si la categoría no existe.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> EditarCategoria(int id)
    {
        var categoria = await _documentoService.ObtenerCategoria(id);
        if (categoria is null)
        {
            return NotFound();
        }

        return View(new CategoriaFormViewModel
        {
            Id = categoria.Id,
            Nombre = categoria.Nombre,
            EsFinanciero = categoria.EsFinanciero,
            EsPublica = categoria.EsPublica
        });
    }

    /// <summary>
    /// Guarda los cambios del nombre de una categoría (Requisito 37.2). Devuelve
    /// 404 si la categoría no existe.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarCategoria(CategoriaFormViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var actualizada = await _documentoService.EditarCategoria(
            new CategoriaDocumento
            {
                Id = modelo.Id,
                Nombre = modelo.Nombre,
                EsFinanciero = modelo.EsFinanciero,
                EsPublica = modelo.EsPublica
            });
        if (actualizada is null)
        {
            return NotFound();
        }

        await RegistrarActividad(
            "Editar",
            ModuloCategoriaDocumento,
            $"Categoría de documentos '{modelo.Nombre}' (Id {modelo.Id})");
        TempData["Mensaje"] = "Categoría actualizada correctamente.";
        return RedirectToAction(nameof(Categorias));
    }

    /// <summary>
    /// Elimina una categoría y, en cascada, sus documentos asociados; además
    /// elimina los archivos físicos de dichos documentos (Requisito 37.3). La
    /// política <c>EliminarContenido</c> restringe la acción al Administrador
    /// General, por lo que un Editor recibe un rechazo de autorización
    /// (Requisito 37.4).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "EliminarContenido")]
    public async Task<IActionResult> EliminarCategoria(int id)
    {
        var categoria = await _documentoService.ObtenerCategoria(id);
        if (categoria is null)
        {
            return NotFound();
        }

        // Captura las rutas de los archivos de la categoría para borrarlos tras el
        // borrado en cascada de los documentos.
        var rutas = categoria.Documentos
            .Select(d => d.ArchivoRuta)
            .Where(r => !string.IsNullOrEmpty(r))
            .ToList();

        var eliminada = await _documentoService.EliminarCategoria(id);
        if (!eliminada)
        {
            return NotFound();
        }

        foreach (var ruta in rutas)
        {
            _archivoService.Eliminar(ruta);
        }

        await RegistrarActividad(
            "Eliminar",
            ModuloCategoriaDocumento,
            $"Categoría de documentos '{categoria.Nombre}' (Id {id})");
        TempData["Mensaje"] = "Categoría eliminada correctamente.";
        return RedirectToAction(nameof(Categorias));
    }

    // ----- Gestión de documentos -----

    /// <summary>
    /// Muestra el formulario de carga de un documento (Requisito 38.1).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Subir()
    {
        await PoblarCategorias();
        return View(new DocumentoFormViewModel());
    }

    /// <summary>
    /// Sube un documento con título, descripción y categoría, guardando el archivo
    /// con <see cref="IArchivoService"/> y persistiendo la ruta relativa en
    /// <see cref="Documento.ArchivoRuta"/> (Requisito 38.1).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Subir(DocumentoFormViewModel modelo)
    {
        if (modelo.Archivo is not { Length: > 0 })
        {
            ModelState.AddModelError(nameof(modelo.Archivo), "El archivo es obligatorio.");
        }

        if (!ModelState.IsValid)
        {
            await PoblarCategorias(modelo.CategoriaDocumentoId);
            return View(modelo);
        }

        string archivoRuta;
        try
        {
            archivoRuta = await _archivoService.Guardar(
                modelo.Archivo!, await SubcarpetaSegunCategoria(modelo.CategoriaDocumentoId));
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(nameof(modelo.Archivo), ex.Message);
            await PoblarCategorias(modelo.CategoriaDocumentoId);
            return View(modelo);
        }

        var documento = new Documento
        {
            Titulo = modelo.Titulo,
            Descripcion = modelo.Descripcion,
            ArchivoRuta = archivoRuta,
            CategoriaDocumentoId = modelo.CategoriaDocumentoId
        };

        await _documentoService.CrearDocumento(documento);
        await RegistrarActividad("Crear", ModuloDocumento, $"Documento '{documento.Titulo}'");
        TempData["Mensaje"] = "Documento subido correctamente.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Muestra el formulario de edición de un documento existente (Requisito 39.1).
    /// Devuelve 404 si el documento no existe.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var documento = await _documentoService.ObtenerDocumento(id);
        if (documento is null)
        {
            return NotFound();
        }

        var modelo = new DocumentoFormViewModel
        {
            Id = documento.Id,
            Titulo = documento.Titulo,
            Descripcion = documento.Descripcion,
            CategoriaDocumentoId = documento.CategoriaDocumentoId,
            ArchivoRutaActual = documento.ArchivoRuta
        };

        await PoblarCategorias(documento.CategoriaDocumentoId);
        return View(modelo);
    }

    /// <summary>
    /// Guarda los cambios de un documento (Requisito 39.1). Si se sube un archivo
    /// nuevo, lo guarda con <see cref="IArchivoService"/>, actualiza la ruta y
    /// elimina el archivo anterior; si no se sube ninguno, conserva el existente
    /// (Requisito 39.2). Devuelve 404 si el documento no existe.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(DocumentoFormViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            await PoblarCategorias(modelo.CategoriaDocumentoId);
            return View(modelo);
        }

        var existente = await _documentoService.ObtenerDocumento(modelo.Id);
        if (existente is null)
        {
            return NotFound();
        }

        var archivoRuta = existente.ArchivoRuta;
        var rutaAnterior = existente.ArchivoRuta;
        var subcarpetaDestino = await SubcarpetaSegunCategoria(modelo.CategoriaDocumentoId);

        if (modelo.Archivo is { Length: > 0 })
        {
            try
            {
                archivoRuta = await _archivoService.Guardar(modelo.Archivo, subcarpetaDestino);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(nameof(modelo.Archivo), ex.Message);
                modelo.ArchivoRutaActual = existente.ArchivoRuta;
                await PoblarCategorias(modelo.CategoriaDocumentoId);
                return View(modelo);
            }
        }
        else if (!string.IsNullOrEmpty(rutaAnterior))
        {
            // No se subió archivo nuevo, pero puede haber cambiado la privacidad de
            // la categoría destino. Si el archivo no está en la carpeta correcta
            // (pública vs privada), se reubica para que la protección de acceso sea
            // coherente con el nuevo estado de la categoría.
            var carpetaActual = rutaAnterior.Contains($"/{SubcarpetaDocumentosPrivados}/", StringComparison.OrdinalIgnoreCase)
                ? SubcarpetaDocumentosPrivados
                : SubcarpetaDocumentos;
            if (!string.Equals(carpetaActual, subcarpetaDestino, StringComparison.OrdinalIgnoreCase))
            {
                archivoRuta = _archivoService.Mover(rutaAnterior, subcarpetaDestino);
            }
        }

        var actualizado = new Documento
        {
            Id = existente.Id,
            Titulo = modelo.Titulo,
            Descripcion = modelo.Descripcion,
            ArchivoRuta = archivoRuta,
            CategoriaDocumentoId = modelo.CategoriaDocumentoId
        };

        await _documentoService.EditarDocumento(actualizado);

        // Si se reemplazó el archivo, elimina el archivo físico anterior.
        if (modelo.Archivo is { Length: > 0 } && !string.IsNullOrEmpty(rutaAnterior))
        {
            _archivoService.Eliminar(rutaAnterior);
        }

        await RegistrarActividad("Editar", ModuloDocumento, $"Documento '{actualizado.Titulo}' (Id {actualizado.Id})");
        TempData["Mensaje"] = "Documento actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Elimina un documento y su archivo físico asociado (Requisito 39.2). La
    /// política <c>EliminarContenido</c> restringe la acción al Administrador
    /// General, por lo que un Editor recibe un rechazo de autorización
    /// (Requisito 39.3).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "EliminarContenido")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var documento = await _documentoService.ObtenerDocumento(id);
        if (documento is null)
        {
            return NotFound();
        }

        var archivoRuta = documento.ArchivoRuta;
        await _documentoService.EliminarDocumento(id);

        if (!string.IsNullOrEmpty(archivoRuta))
        {
            _archivoService.Eliminar(archivoRuta);
        }

        await RegistrarActividad("Eliminar", ModuloDocumento, $"Documento '{documento.Titulo}' (Id {id})");
        TempData["Mensaje"] = "Documento eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Puebla <c>ViewBag.Categorias</c> con la lista de categorías de documentos
    /// para el desplegable de los formularios de carga y edición (Requisito 38.1).
    /// </summary>
    /// <param name="seleccionada">Identificador de la categoría preseleccionada.</param>
    private async Task PoblarCategorias(int? seleccionada = null)
    {
        var categorias = await _documentoService.ListarCategorias();
        ViewBag.Categorias = new SelectList(categorias, nameof(CategoriaDocumento.Id),
            nameof(CategoriaDocumento.Nombre), seleccionada);
    }

    /// <summary>
    /// Devuelve la subcarpeta de almacenamiento según la privacidad de la categoría
    /// destino: <c>documentos</c> para categorías públicas (servidas como estáticos)
    /// y <c>documentos-privados</c> para categorías privadas (bloqueadas del acceso
    /// estático y descargables solo desde el Panel).
    /// </summary>
    private async Task<string> SubcarpetaSegunCategoria(int categoriaId)
    {
        var categoria = await _documentoService.ObtenerCategoria(categoriaId);
        return categoria is { EsPublica: false }
            ? SubcarpetaDocumentosPrivados
            : SubcarpetaDocumentos;
    }

    /// <summary>
    /// Descarga el archivo de cualquier documento desde el Panel, incluidos los de
    /// categorías privadas que no se sirven como estáticos. Requiere autenticación
    /// (todo el controlador exige <c>[Authorize]</c>). Devuelve 404 si el documento
    /// o su archivo físico no existen.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> DescargarPrivado(int id)
    {
        var documento = await _documentoService.ObtenerDocumento(id);
        if (documento is null)
        {
            return NotFound();
        }

        ArchivoAbierto abierto;
        try
        {
            abierto = _archivoService.Abrir(documento.ArchivoRuta);
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }

        return File(abierto.Stream, abierto.ContentType, abierto.Nombre);
    }

    /// <summary>
    /// Anota una acción de contenido en el registro de actividad (veeduría) a nombre
    /// del administrador autenticado, tomando su identificador del claim
    /// <see cref="ClaimTypes.NameIdentifier"/> (Requisito 5.1). Recibe el módulo
    /// afectado (<c>Documento</c> o <c>CategoriaDocumento</c>). Si no se puede
    /// resolver un identificador válido, no registra nada. La escritura es resiliente
    /// por diseño del servicio y no interrumpe la operación principal.
    /// </summary>
    private async Task RegistrarActividad(string accion, string modulo, string detalle)
    {
        if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var administradorId))
        {
            await _registroActividad.Registrar(administradorId, accion, modulo, detalle);
        }
    }
}
