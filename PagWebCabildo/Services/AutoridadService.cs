using Microsoft.EntityFrameworkCore;
using PagWebCabildo.Data;
using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Services;

/// <summary>
/// Implementación de <see cref="IAutoridadService"/> sobre <see cref="AppDbContext"/>.
/// El método <see cref="Listar"/> es la única fuente de verdad del orden de las
/// autoridades (Property 5): siempre devuelve el conjunto ordenado de forma
/// ascendente por <see cref="Autoridad.Orden"/>.
/// </summary>
public sealed class AutoridadService : IAutoridadService
{
    private readonly AppDbContext _db;

    public AutoridadService(AppDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Autoridad>> Listar()
    {
        // OrderBy ascendente por Orden: fuente de verdad de la Property 5
        // (Requisitos 4.2 y 25.3).
        return await _db.Autoridades
            .OrderBy(a => a.Orden)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Autoridad?> Obtener(int id)
    {
        return await _db.Autoridades.FindAsync(id);
    }

    /// <inheritdoc />
    public async Task<Autoridad> Crear(Autoridad autoridad)
    {
        _db.Autoridades.Add(autoridad);
        await _db.SaveChangesAsync();
        return autoridad;
    }

    /// <inheritdoc />
    public async Task Editar(Autoridad autoridad)
    {
        _db.Autoridades.Update(autoridad);
        await _db.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task Eliminar(int id)
    {
        var autoridad = await _db.Autoridades.FindAsync(id);
        if (autoridad is null)
        {
            return;
        }

        _db.Autoridades.Remove(autoridad);
        await _db.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task ReordenarAutoridades(IEnumerable<(int id, int orden)> ordenes)
    {
        var nuevosOrdenes = ordenes
            .GroupBy(o => o.id)
            .ToDictionary(g => g.Key, g => g.Last().orden);

        if (nuevosOrdenes.Count == 0)
        {
            return;
        }

        var ids = nuevosOrdenes.Keys.ToList();
        var autoridades = await _db.Autoridades
            .Where(a => ids.Contains(a.Id))
            .ToListAsync();

        foreach (var autoridad in autoridades)
        {
            autoridad.Orden = nuevosOrdenes[autoridad.Id];
        }

        await _db.SaveChangesAsync();
    }
}
