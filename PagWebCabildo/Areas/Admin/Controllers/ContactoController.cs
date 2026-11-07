using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PagWebCabildo.Areas.Admin.Models;
using PagWebCabildo.Services;

namespace PagWebCabildo.Areas.Admin.Controllers;

/// <summary>
/// Controlador de edición de la información de contacto del Panel de Administración
/// (Area Admin). Es distinto del controlador público <c>ContactoController</c>:
/// permite a cualquier administrador autenticado editar los datos de contacto del
/// resguardo (Requisito 40.1), que operan sobre la entidad singleton
/// <c>InformacionContacto</c> a través de <see cref="IContactoService"/>.
/// </summary>
[Area("Admin")]
[Authorize(Policy = "GestionContenidoGeneral")]
public sealed class ContactoController : Controller
{
    private const string ModuloContacto = "Contacto";

    private readonly IContactoService _contactoService;
    private readonly IRegistroActividadService _registroActividad;

    public ContactoController(
        IContactoService contactoService,
        IRegistroActividadService registroActividad)
    {
        _contactoService = contactoService;
        _registroActividad = registroActividad;
    }

    /// <summary>
    /// Muestra el formulario de edición de la información de contacto con los datos
    /// actuales cargados (Requisito 40.1).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Editar()
    {
        var informacion = await _contactoService.ObtenerInformacion();
        var modelo = new ContactoAdminViewModel
        {
            Direccion = informacion.Direccion,
            Telefono = informacion.Telefono,
            CorreoElectronico = informacion.CorreoElectronico,
            OtrosDatos = informacion.OtrosDatos
        };
        return View(modelo);
    }

    /// <summary>
    /// Persiste los nuevos datos de contacto sobre el registro singleton y vuelve a
    /// mostrar el formulario con un mensaje de éxito (Requisito 40.1).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(ContactoAdminViewModel modelo)
    {
        var informacion = await _contactoService.ActualizarInformacion(
            modelo.Direccion ?? string.Empty,
            modelo.Telefono ?? string.Empty,
            modelo.CorreoElectronico ?? string.Empty,
            modelo.OtrosDatos ?? string.Empty);

        modelo.Direccion = informacion.Direccion;
        modelo.Telefono = informacion.Telefono;
        modelo.CorreoElectronico = informacion.CorreoElectronico;
        modelo.OtrosDatos = informacion.OtrosDatos;

        await RegistrarActividad("Editar", "Información de contacto del resguardo");

        TempData["Mensaje"] = "La información de contacto se actualizó correctamente.";
        return View(modelo);
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
            await _registroActividad.Registrar(administradorId, accion, ModuloContacto, detalle);
        }
    }
}
