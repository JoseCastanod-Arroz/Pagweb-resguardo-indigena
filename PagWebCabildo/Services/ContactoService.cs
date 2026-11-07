using Microsoft.EntityFrameworkCore;
using PagWebCabildo.Data;
using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Services;

/// <summary>
/// Implementación de <see cref="IContactoService"/> sobre <see cref="AppDbContext"/>.
/// Trata <see cref="InformacionContacto"/> como un registro singleton: siempre
/// devuelve y actualiza el primer (y único) registro de la tabla.
/// </summary>
public sealed class ContactoService : IContactoService
{
    private readonly AppDbContext _db;

    public ContactoService(AppDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<InformacionContacto> ObtenerInformacion()
    {
        var informacion = await _db.InformacionesContacto
            .OrderBy(i => i.Id)
            .FirstOrDefaultAsync();

        if (informacion is null)
        {
            // El seeding garantiza un registro, pero mantenemos la invariante de
            // registro único creando uno por defecto si la tabla estuviera vacía.
            informacion = new InformacionContacto();
            _db.InformacionesContacto.Add(informacion);
            await _db.SaveChangesAsync();
        }

        return informacion;
    }

    /// <inheritdoc />
    public async Task<InformacionContacto> ActualizarInformacion(
        string direccion,
        string telefono,
        string correoElectronico,
        string otrosDatos)
    {
        var informacion = await ObtenerInformacion();

        informacion.Direccion = direccion ?? string.Empty;
        informacion.Telefono = telefono ?? string.Empty;
        informacion.CorreoElectronico = correoElectronico ?? string.Empty;
        informacion.OtrosDatos = otrosDatos ?? string.Empty;

        await _db.SaveChangesAsync();
        return informacion;
    }
}
