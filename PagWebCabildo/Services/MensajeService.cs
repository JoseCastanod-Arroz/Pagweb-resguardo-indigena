using Microsoft.EntityFrameworkCore;
using PagWebCabildo.Data;
using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Services;

/// <summary>
/// Implementación de <see cref="IMensajeService"/> sobre <see cref="AppDbContext"/>.
/// Concentra la validación del formulario de contacto, el orden del listado de
/// administración (del más reciente al más antiguo) y las operaciones de detalle,
/// marcar como atendido y eliminación de mensajes.
/// </summary>
public sealed class MensajeService : IMensajeService
{
    private readonly AppDbContext _db;
    private readonly IValidacionContactoService _validacionContacto;

    public MensajeService(AppDbContext db, IValidacionContactoService validacionContacto)
    {
        _db = db;
        _validacionContacto = validacionContacto;
    }

    /// <inheritdoc />
    public async Task<ResultadoRegistroMensaje> RegistrarMensaje(
        string nombre,
        string medioContacto,
        string asunto,
        string contenido)
    {
        // Validación previa a cualquier persistencia: si alguna regla falla, no se
        // registra ningún mensaje (Propiedad 9, Req 16.4).
        var errores = new List<string>();

        var nombreRecortado = (nombre ?? string.Empty).Trim();
        var medioRecortado = (medioContacto ?? string.Empty).Trim();
        var asuntoRecortado = (asunto ?? string.Empty).Trim();
        var contenidoRecortado = (contenido ?? string.Empty).Trim();

        // Campos obligatorios: vacíos o solo con espacios se rechazan (Req 16.4).
        if (nombreRecortado.Length == 0)
        {
            errores.Add("El nombre es obligatorio.");
        }
        else if (nombreRecortado.Length > IMensajeService.NombreMaxLongitud)
        {
            errores.Add($"El nombre no puede superar los {IMensajeService.NombreMaxLongitud} caracteres.");
        }

        if (medioRecortado.Length == 0)
        {
            errores.Add("El medio de contacto es obligatorio.");
        }
        else if (!_validacionContacto.EsMedioContactoValido(medioRecortado))
        {
            // Medio con formato inválido (Req 16.2, 16.5).
            errores.Add("El medio de contacto no tiene un formato válido (correo electrónico o teléfono de 7 a 15 dígitos).");
        }

        if (asuntoRecortado.Length == 0)
        {
            errores.Add("El asunto es obligatorio.");
        }
        else if (asuntoRecortado.Length > IMensajeService.AsuntoMaxLongitud)
        {
            errores.Add($"El asunto no puede superar los {IMensajeService.AsuntoMaxLongitud} caracteres.");
        }

        if (contenidoRecortado.Length == 0)
        {
            errores.Add("El mensaje es obligatorio.");
        }
        else if (contenidoRecortado.Length > IMensajeService.ContenidoMaxLongitud)
        {
            errores.Add($"El mensaje no puede superar los {IMensajeService.ContenidoMaxLongitud} caracteres.");
        }

        if (errores.Count > 0)
        {
            // Rechazo sin registrar ningún mensaje (Propiedad 9).
            return new ResultadoRegistroMensaje(false, errores, null);
        }

        // Registro del mensaje recuperable con datos que coinciden con lo ingresado
        // (Propiedad 10, Req 16.3, 17.1).
        var mensaje = new Mensaje
        {
            Nombre = nombreRecortado,
            MedioContacto = medioRecortado,
            Asunto = asuntoRecortado,
            Contenido = contenidoRecortado,
            FechaRecepcion = DateTime.Now,
            Atendido = false
        };

        _db.Mensajes.Add(mensaje);
        await _db.SaveChangesAsync();

        return new ResultadoRegistroMensaje(true, Array.Empty<string>(), mensaje);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Mensaje>> ListarRecientesPrimero()
    {
        // Fuente única de verdad del orden del listado de mensajes: del más reciente
        // al más antiguo (Propiedad 4, Req 41.1, 41.2).
        return await _db.Mensajes
            .OrderByDescending(m => m.FechaRecepcion)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Mensaje?> Ver(int id)
    {
        return await _db.Mensajes.FindAsync(id);
    }

    /// <inheritdoc />
    public async Task<Mensaje?> MarcarAtendido(int id)
    {
        var mensaje = await _db.Mensajes.FindAsync(id);
        if (mensaje is null)
        {
            return null;
        }

        mensaje.Atendido = true;
        await _db.SaveChangesAsync();
        return mensaje;
    }

    /// <inheritdoc />
    public async Task<bool> Eliminar(int id)
    {
        var mensaje = await _db.Mensajes.FindAsync(id);
        if (mensaje is null)
        {
            return false;
        }

        _db.Mensajes.Remove(mensaje);
        await _db.SaveChangesAsync();
        return true;
    }
}
