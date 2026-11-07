using Microsoft.EntityFrameworkCore;
using PagWebCabildo.Data;
using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Services;

/// <summary>
/// Implementación de <see cref="IPresentacionService"/> sobre <see cref="AppDbContext"/>.
/// Trata <see cref="PresentacionResguardo"/> como un registro singleton: siempre
/// devuelve y actualiza el primer (y único) registro de la tabla.
/// </summary>
public sealed class PresentacionService : IPresentacionService
{
    private readonly AppDbContext _db;

    public PresentacionService(AppDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<PresentacionResguardo> ObtenerPresentacion()
    {
        var presentacion = await _db.Presentaciones
            .OrderBy(p => p.Id)
            .FirstOrDefaultAsync();

        if (presentacion is null)
        {
            // El seeding garantiza un registro, pero mantenemos la invariante de
            // registro único creando uno por defecto si la tabla estuviera vacía.
            presentacion = new PresentacionResguardo { Texto = string.Empty };
            _db.Presentaciones.Add(presentacion);
            await _db.SaveChangesAsync();
        }

        return presentacion;
    }

    /// <inheritdoc />
    public async Task<PresentacionResguardo> ActualizarPresentacion(string texto)
    {
        var presentacion = await ObtenerPresentacion();
        presentacion.Texto = texto ?? string.Empty;
        await _db.SaveChangesAsync();
        return presentacion;
    }
}
