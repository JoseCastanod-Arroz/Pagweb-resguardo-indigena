using Microsoft.EntityFrameworkCore;
using PagWebCabildo.Data;
using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Services;

/// <summary>
/// Implementación de <see cref="IComunicadoService"/> sobre <see cref="AppDbContext"/>.
/// Concentra las reglas de visibilidad de borradores, el orden público y las
/// transiciones de estado de los comunicados.
/// </summary>
public sealed class ComunicadoService : IComunicadoService
{
    private readonly AppDbContext _db;

    public ComunicadoService(AppDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Comunicado>> ListarPublicadosRecientesPrimero()
    {
        // Fuente única de verdad del orden público: solo publicados y del más
        // reciente al más antiguo (Propiedad 2, Req 8.2). Al filtrar por Publicado
        // también se garantiza que los borradores no aparezcan (Propiedad 3,
        // Req 8.1, 32.3).
        return await _db.Comunicados
            .Where(c => c.Estado == EstadoComunicado.Publicado)
            .OrderByDescending(c => c.Fecha)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Comunicado?> ObtenerPublicado(int id)
    {
        // Un borrador nunca es accesible por enlace directo en el sitio público
        // (Propiedad 3, Req 32.3): solo se devuelve si está publicado.
        return await _db.Comunicados
            .FirstOrDefaultAsync(c => c.Id == id && c.Estado == EstadoComunicado.Publicado);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Comunicado>> ListarTodos()
    {
        return await _db.Comunicados
            .OrderByDescending(c => c.Fecha)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Comunicado?> Obtener(int id)
    {
        return await _db.Comunicados.FindAsync(id);
    }

    /// <inheritdoc />
    public async Task<Comunicado> Crear(string titulo, DateTime fecha, string contenido, EstadoComunicado estado)
    {
        var comunicado = new Comunicado
        {
            Titulo = titulo ?? string.Empty,
            Fecha = fecha,
            Contenido = contenido ?? string.Empty,
            Estado = estado
        };

        _db.Comunicados.Add(comunicado);
        await _db.SaveChangesAsync();
        return comunicado;
    }

    /// <inheritdoc />
    public async Task<Comunicado> Editar(int id, string titulo, DateTime fecha, string contenido)
    {
        var comunicado = await _db.Comunicados.FindAsync(id)
            ?? throw new InvalidOperationException($"No existe un comunicado con Id {id}.");

        comunicado.Titulo = titulo ?? string.Empty;
        comunicado.Fecha = fecha;
        comunicado.Contenido = contenido ?? string.Empty;

        await _db.SaveChangesAsync();
        return comunicado;
    }

    /// <inheritdoc />
    public async Task<bool> Eliminar(int id)
    {
        var comunicado = await _db.Comunicados.FindAsync(id);
        if (comunicado is null)
        {
            return false;
        }

        _db.Comunicados.Remove(comunicado);
        await _db.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<Comunicado> GuardarBorrador(int? id, string titulo, DateTime fecha, string contenido)
    {
        // Validación de borrador (Req 32.2): título y contenido son obligatorios.
        // Se señala con un mensaje que enumera los campos faltantes para que la
        // capa de presentación los muestre.
        var camposFaltantes = new List<string>();
        if (string.IsNullOrWhiteSpace(titulo))
        {
            camposFaltantes.Add("Título");
        }
        if (string.IsNullOrWhiteSpace(contenido))
        {
            camposFaltantes.Add("Contenido");
        }
        if (camposFaltantes.Count > 0)
        {
            throw new ArgumentException(
                $"No se puede guardar el borrador. Campos obligatorios faltantes: {string.Join(", ", camposFaltantes)}.");
        }

        Comunicado comunicado;
        if (id is int existenteId)
        {
            comunicado = await _db.Comunicados.FindAsync(existenteId)
                ?? throw new InvalidOperationException($"No existe un comunicado con Id {existenteId}.");

            comunicado.Titulo = titulo;
            comunicado.Fecha = fecha;
            comunicado.Contenido = contenido;
            comunicado.Estado = EstadoComunicado.Borrador;
        }
        else
        {
            comunicado = new Comunicado
            {
                Titulo = titulo,
                Fecha = fecha,
                Contenido = contenido,
                Estado = EstadoComunicado.Borrador
            };
            _db.Comunicados.Add(comunicado);
        }

        await _db.SaveChangesAsync();
        return comunicado;
    }

    /// <inheritdoc />
    public async Task<Comunicado> Publicar(int id)
    {
        var comunicado = await _db.Comunicados.FindAsync(id)
            ?? throw new InvalidOperationException($"No existe un comunicado con Id {id}.");

        // Publicar solo procede desde el estado borrador (Propiedad 11,
        // Req 32.4/32.5/32.6). En cualquier otro estado se rechaza y el estado
        // permanece sin cambios.
        if (comunicado.Estado != EstadoComunicado.Borrador)
        {
            throw new InvalidOperationException(
                "El comunicado no puede publicarse desde su estado actual; solo se puede publicar un borrador.");
        }

        comunicado.Estado = EstadoComunicado.Publicado;

        // La fecha representa la fecha del comunicado; si no se fijó al crear el
        // borrador, se establece a la fecha de publicación.
        if (comunicado.Fecha == default)
        {
            comunicado.Fecha = DateTime.Now;
        }

        await _db.SaveChangesAsync();
        return comunicado;
    }
}
