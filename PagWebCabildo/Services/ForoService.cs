using Microsoft.EntityFrameworkCore;
using PagWebCabildo.Data;
using PagWebCabildo.Models.Entidades;
using PagWebCabildo.Models.ViewModels;

namespace PagWebCabildo.Services;

/// <summary>
/// Implementación de <see cref="IForoService"/> sobre <see cref="AppDbContext"/>.
/// Concentra el CRUD de publicaciones y comentarios del foro comunitario y la
/// lógica de moderación (el contenido público es únicamente el aprobado).
/// </summary>
public sealed class ForoService : IForoService
{
    private readonly AppDbContext _contexto;

    public ForoService(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    // ----- Consultas públicas -----

    /// <inheritdoc />
    public async Task<ForoFeedPaginado> ListarAprobadas(
        string? busqueda = null,
        TipoPublicacionForo? tipo = null,
        OrdenForo orden = OrdenForo.Recientes,
        int pagina = 1,
        int tamañoPagina = 10)
    {
        // Saneado de los parámetros de paginación.
        if (pagina < 1) pagina = 1;
        if (tamañoPagina < 1) tamañoPagina = 10;

        var consulta = _contexto.PublicacionesForo
            .Where(p => p.Estado == EstadoPublicacionForo.Aprobado);

        if (tipo is not null)
        {
            consulta = consulta.Where(p => p.Tipo == tipo);
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            // Búsqueda sin distinción de mayúsculas sobre título, contenido y autor.
            var termino = busqueda.Trim();
            consulta = consulta.Where(p =>
                EF.Functions.Like(p.Titulo, $"%{termino}%") ||
                EF.Functions.Like(p.Contenido, $"%{termino}%") ||
                EF.Functions.Like(p.Autor, $"%{termino}%"));
        }

        // Total antes de paginar, para calcular el número de páginas.
        var total = await consulta.CountAsync();

        // Proyección con el conteo de comentarios aprobados.
        var proyeccion = consulta.Select(p => new PublicacionForoFeedItem
        {
            Publicacion = p,
            ComentariosAprobados = p.Comentarios
                .Count(c => c.Estado == EstadoPublicacionForo.Aprobado)
        });

        // Ordenamiento según el criterio elegido (con desempate por fecha).
        proyeccion = orden == OrdenForo.Comentados
            ? proyeccion
                .OrderByDescending(x => x.ComentariosAprobados)
                .ThenByDescending(x => x.Publicacion.FechaEnvio)
            : proyeccion
                .OrderByDescending(x => x.Publicacion.FechaEnvio);

        var items = await proyeccion
            .Skip((pagina - 1) * tamañoPagina)
            .Take(tamañoPagina)
            .ToListAsync();

        return new ForoFeedPaginado
        {
            Items = items,
            TotalItems = total,
            Pagina = pagina,
            TamañoPagina = tamañoPagina,
            Busqueda = busqueda,
            Tipo = tipo,
            Orden = orden
        };
    }

    /// <inheritdoc />
    public async Task<PublicacionForo?> ObtenerAprobadaConComentarios(int id)
    {
        var publicacion = await _contexto.PublicacionesForo
            .Include(p => p.Comentarios
                .Where(c => c.Estado == EstadoPublicacionForo.Aprobado)
                .OrderBy(c => c.FechaEnvio))
            .FirstOrDefaultAsync(p => p.Id == id && p.Estado == EstadoPublicacionForo.Aprobado);

        return publicacion;
    }

    // ----- Envío por visitantes anónimos -----

    /// <inheritdoc />
    public async Task<PublicacionForo> CrearPublicacion(PublicacionForo publicacion)
    {
        publicacion.Estado = EstadoPublicacionForo.Pendiente;
        publicacion.FechaEnvio = DateTime.Now;
        _contexto.PublicacionesForo.Add(publicacion);
        await _contexto.SaveChangesAsync();
        return publicacion;
    }

    /// <inheritdoc />
    public async Task<ComentarioForo?> CrearComentario(ComentarioForo comentario)
    {
        // Solo se admiten comentarios sobre publicaciones existentes y aprobadas.
        var publicacionAprobada = await _contexto.PublicacionesForo
            .AnyAsync(p => p.Id == comentario.PublicacionForoId
                && p.Estado == EstadoPublicacionForo.Aprobado);
        if (!publicacionAprobada)
        {
            return null;
        }

        comentario.Estado = EstadoPublicacionForo.Pendiente;
        comentario.FechaEnvio = DateTime.Now;
        _contexto.ComentariosForo.Add(comentario);
        await _contexto.SaveChangesAsync();
        return comentario;
    }

    // ----- Moderación -----

    /// <inheritdoc />
    public async Task<IReadOnlyList<PublicacionForo>> ListarTodasPublicaciones()
    {
        return await _contexto.PublicacionesForo
            .OrderByDescending(p => p.FechaEnvio)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ComentarioForo>> ListarComentariosPendientes()
    {
        return await _contexto.ComentariosForo
            .Include(c => c.PublicacionForo)
            .Where(c => c.Estado == EstadoPublicacionForo.Pendiente)
            .OrderBy(c => c.FechaEnvio)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<PublicacionForo?> ObtenerPublicacionConComentarios(int id)
    {
        return await _contexto.PublicacionesForo
            .Include(p => p.Comentarios.OrderBy(c => c.FechaEnvio))
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <inheritdoc />
    public async Task<PublicacionForo?> ObtenerPublicacion(int id)
    {
        return await _contexto.PublicacionesForo.FindAsync(id);
    }

    /// <inheritdoc />
    public async Task<PublicacionForo?> CambiarEstadoPublicacion(int id, EstadoPublicacionForo estado)
    {
        var existente = await _contexto.PublicacionesForo.FindAsync(id);
        if (existente is null)
        {
            return null;
        }

        existente.Estado = estado;
        await _contexto.SaveChangesAsync();
        return existente;
    }

    /// <inheritdoc />
    public async Task<bool> EliminarPublicacion(int id)
    {
        var existente = await _contexto.PublicacionesForo.FindAsync(id);
        if (existente is null)
        {
            return false;
        }

        _contexto.PublicacionesForo.Remove(existente);
        await _contexto.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<ComentarioForo?> CambiarEstadoComentario(int id, EstadoPublicacionForo estado)
    {
        var existente = await _contexto.ComentariosForo.FindAsync(id);
        if (existente is null)
        {
            return null;
        }

        existente.Estado = estado;
        await _contexto.SaveChangesAsync();
        return existente;
    }

    /// <inheritdoc />
    public async Task<bool> EliminarComentario(int id)
    {
        var existente = await _contexto.ComentariosForo.FindAsync(id);
        if (existente is null)
        {
            return false;
        }

        _contexto.ComentariosForo.Remove(existente);
        await _contexto.SaveChangesAsync();
        return true;
    }
}
