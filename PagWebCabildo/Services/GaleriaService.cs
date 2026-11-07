using Microsoft.EntityFrameworkCore;
using PagWebCabildo.Data;
using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Services;

/// <summary>
/// Implementación de <see cref="IGaleriaService"/> sobre <see cref="AppDbContext"/>.
/// Cubre el CRUD de temas y elementos de la galería y la asociación de cada
/// elemento a su tema (Requisitos 10, 11, 12, 33, 34, 35 y 36).
/// </summary>
public sealed class GaleriaService : IGaleriaService
{
    private readonly AppDbContext _contexto;

    /// <summary>
    /// Crea el servicio con el contexto de datos inyectado.
    /// </summary>
    /// <param name="contexto">Contexto de EF Core del sitio del resguardo.</param>
    public GaleriaService(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    // --- Temas ---

    /// <inheritdoc />
    public async Task<IReadOnlyList<TemaGaleria>> ListarTemas()
    {
        return await _contexto.TemasGaleria
            .OrderBy(t => t.Nombre)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<TemaGaleria?> ObtenerTema(int id)
    {
        // Se incluyen los elementos para la vista de tema (fotos y videos).
        return await _contexto.TemasGaleria
            .Include(t => t.Elementos)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    /// <inheritdoc />
    public async Task<TemaGaleria> CrearTema(TemaGaleria tema)
    {
        _contexto.TemasGaleria.Add(tema);
        await _contexto.SaveChangesAsync();
        return tema;
    }

    /// <inheritdoc />
    public async Task<TemaGaleria?> EditarTema(TemaGaleria tema)
    {
        var existente = await _contexto.TemasGaleria.FindAsync(tema.Id);
        if (existente is null)
        {
            return null;
        }

        existente.Nombre = tema.Nombre;
        existente.Descripcion = tema.Descripcion;
        existente.PortadaRuta = tema.PortadaRuta;

        await _contexto.SaveChangesAsync();
        return existente;
    }

    /// <inheritdoc />
    public async Task<bool> EliminarTema(int id)
    {
        var existente = await _contexto.TemasGaleria.FindAsync(id);
        if (existente is null)
        {
            return false;
        }

        _contexto.TemasGaleria.Remove(existente);
        await _contexto.SaveChangesAsync();
        return true;
    }

    // --- Elementos ---

    /// <inheritdoc />
    public async Task<ElementoGaleria?> ObtenerElemento(int id)
    {
        return await _contexto.ElementosGaleria.FindAsync(id);
    }

    /// <inheritdoc />
    public async Task<ElementoGaleria> AgregarElemento(ElementoGaleria elemento)
    {
        // La asociación al tema se materializa a través de TemaGaleriaId.
        _contexto.ElementosGaleria.Add(elemento);
        await _contexto.SaveChangesAsync();
        return elemento;
    }

    /// <inheritdoc />
    public async Task<ElementoGaleria?> EditarElemento(ElementoGaleria elemento)
    {
        var existente = await _contexto.ElementosGaleria.FindAsync(elemento.Id);
        if (existente is null)
        {
            return null;
        }

        existente.Tipo = elemento.Tipo;
        existente.ArchivoRuta = elemento.ArchivoRuta;
        existente.Descripcion = elemento.Descripcion;
        existente.TemaGaleriaId = elemento.TemaGaleriaId;

        await _contexto.SaveChangesAsync();
        return existente;
    }

    /// <inheritdoc />
    public async Task<bool> EliminarElemento(int id)
    {
        var existente = await _contexto.ElementosGaleria.FindAsync(id);
        if (existente is null)
        {
            return false;
        }

        _contexto.ElementosGaleria.Remove(existente);
        await _contexto.SaveChangesAsync();
        return true;
    }
}
