using Microsoft.EntityFrameworkCore;
using PagWebCabildo.Data;
using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Services;

/// <summary>
/// Implementación de <see cref="IEventoService"/> sobre <see cref="AppDbContext"/>.
/// Es la única fuente de verdad para la Property 1 del diseño: el ordenamiento de
/// eventos por proximidad de fecha (Requisito 5.2). El resto de operaciones cubre
/// el CRUD, el marcado de cancelación (Requisito 29) y la asociación de imagen.
/// </summary>
public sealed class EventoService : IEventoService
{
    private readonly AppDbContext _contexto;

    /// <summary>
    /// Crea el servicio con el contexto de datos inyectado.
    /// </summary>
    /// <param name="contexto">Contexto de EF Core del sitio del resguardo.</param>
    public EventoService(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Evento>> ListarOrdenadosPorProximidad()
    {
        // Ordenamiento por proximidad: fecha más próxima primero y, a igualdad de
        // fecha, hora más temprana como desempate (Property 1, Requisito 5.2).
        return await _contexto.Eventos
            .OrderBy(e => e.Fecha)
            .ThenBy(e => e.Hora)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Evento?> Obtener(int id)
    {
        return await _contexto.Eventos.FindAsync(id);
    }

    /// <inheritdoc />
    public async Task<Evento> Crear(Evento evento)
    {
        _contexto.Eventos.Add(evento);
        await _contexto.SaveChangesAsync();
        return evento;
    }

    /// <inheritdoc />
    public async Task<Evento?> Editar(Evento evento)
    {
        var existente = await _contexto.Eventos.FindAsync(evento.Id);
        if (existente is null)
        {
            return null;
        }

        existente.Titulo = evento.Titulo;
        existente.Fecha = evento.Fecha;
        existente.Hora = evento.Hora;
        existente.Lugar = evento.Lugar;
        existente.Descripcion = evento.Descripcion;
        existente.ImagenRuta = evento.ImagenRuta;
        existente.Cancelado = evento.Cancelado;

        await _contexto.SaveChangesAsync();
        return existente;
    }

    /// <inheritdoc />
    public async Task<bool> Eliminar(int id)
    {
        var existente = await _contexto.Eventos.FindAsync(id);
        if (existente is null)
        {
            return false;
        }

        _contexto.Eventos.Remove(existente);
        await _contexto.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<Evento?> MarcarCancelado(int id)
    {
        var existente = await _contexto.Eventos.FindAsync(id);
        if (existente is null)
        {
            return null;
        }

        existente.Cancelado = true;
        await _contexto.SaveChangesAsync();
        return existente;
    }

    /// <inheritdoc />
    public async Task<Evento?> AsociarImagen(int id, string? imagenRuta)
    {
        var existente = await _contexto.Eventos.FindAsync(id);
        if (existente is null)
        {
            return null;
        }

        existente.ImagenRuta = imagenRuta;
        await _contexto.SaveChangesAsync();
        return existente;
    }
}
