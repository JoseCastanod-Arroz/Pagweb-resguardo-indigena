using Microsoft.AspNetCore.Mvc;
using PagWebCabildo.Models.ViewModels;
using PagWebCabildo.Services;

namespace PagWebCabildo.Controllers;

/// <summary>
/// Controlador público de la sección de Contacto (Requisitos 15, 16 y 17). Muestra
/// la información de contacto del cabildo junto con el formulario de contacto, y
/// procesa el envío del formulario validando los datos antes de registrar el
/// mensaje.
/// </summary>
public class ContactoController : Controller
{
    private readonly IContactoService _contacto;
    private readonly IMensajeService _mensajes;

    public ContactoController(IContactoService contacto, IMensajeService mensajes)
    {
        _contacto = contacto;
        _mensajes = mensajes;
    }

    /// <summary>
    /// Muestra la información de contacto del cabildo y un formulario vacío
    /// (Requisitos 15.1, 16.1).
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var informacion = await _contacto.ObtenerInformacion();
        var modelo = new ContactoViewModel
        {
            Informacion = informacion,
            Formulario = new ContactoFormViewModel()
        };
        return View(modelo);
    }

    /// <summary>
    /// Procesa el envío del formulario de contacto. Valida los datos con Data
    /// Annotations y, mediante <see cref="IMensajeService"/>, las reglas de dominio
    /// (campos obligatorios, longitudes y validez del medio de contacto). Si la
    /// validación falla, conserva lo ingresado y muestra los errores (Requisitos
    /// 16.4, 16.5); si tiene éxito, registra el mensaje y muestra una confirmación
    /// (Requisitos 16.3, 17.1).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enviar(ContactoFormViewModel formulario)
    {
        formulario ??= new ContactoFormViewModel();

        if (ModelState.IsValid)
        {
            // La validación de dominio (obligatorios, longitudes y medio de
            // contacto) es la fuente de verdad y decide si el mensaje se registra.
            var resultado = await _mensajes.RegistrarMensaje(
                formulario.Nombre,
                formulario.MedioContacto,
                formulario.Asunto,
                formulario.Mensaje);

            if (resultado.Exito)
            {
                // Confirmación tras registro correcto (Requisitos 16.3, 17.1). Se usa
                // el patrón PRG para evitar reenvíos del formulario al refrescar.
                TempData["MensajeExito"] =
                    "Tu mensaje fue enviado correctamente. Gracias por comunicarte con el cabildo.";
                return RedirectToAction(nameof(Index));
            }

            // Se agregan los errores de dominio a ModelState (p. ej. medio de
            // contacto con formato inválido, Requisito 16.5).
            foreach (var error in resultado.Errores)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }

        // Rechazo: se recarga la información y se devuelve la vista con los datos
        // ingresados y los errores (Requisitos 16.4, 16.5).
        var informacion = await _contacto.ObtenerInformacion();
        var modelo = new ContactoViewModel
        {
            Informacion = informacion,
            Formulario = formulario
        };
        return View(nameof(Index), modelo);
    }
}
