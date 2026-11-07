using Microsoft.EntityFrameworkCore;
using PagWebCabildo.Data;
using PagWebCabildo.Models.Entidades;

namespace PagWebCabildo.Services;

/// <summary>
/// Implementación de <see cref="IRegistroActividadService"/> sobre
/// <see cref="AppDbContext"/>. Concentra la escritura resiliente del registro de
/// actividad y su listado descendente por fecha para la veeduría del cabildo
/// (Requisitos 5.1, 5.2, 5.3).
/// </summary>
/// <remarks>
/// La escritura del registro no debe interrumpir la operación principal del
/// llamador: si la persistencia falla, el error se anota en el log y el método
/// retorna con normalidad, de modo que la acción de contenido se complete.
/// </remarks>
public sealed class RegistroActividadService : IRegistroActividadService
{
    private readonly AppDbContext _contexto;
    private readonly ILogger<RegistroActividadService> _logger;

    /// <summary>
    /// Crea el servicio con el contexto de datos y el registrador de logs inyectados.
    /// </summary>
    /// <param name="contexto">Contexto de EF Core del sitio del resguardo.</param>
    /// <param name="logger">Registrador de logs para anotar fallos de la bitácora.</param>
    public RegistroActividadService(AppDbContext contexto, ILogger<RegistroActividadService> logger)
    {
        _contexto = contexto;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task Registrar(int administradorId, string accion, string modulo, string detalle)
    {
        // La escritura del registro es resiliente (manejo de errores del diseño):
        // un fallo al persistir la bitácora nunca debe interrumpir la operación
        // principal del llamador. Por eso se captura cualquier excepción, se anota
        // en el log y se retorna sin propagarla.
        try
        {
            var registro = new RegistroActividad
            {
                AdministradorId = administradorId,
                Accion = accion ?? string.Empty,
                Modulo = modulo ?? string.Empty,
                Detalle = detalle ?? string.Empty,
                FechaHora = DateTime.Now
            };

            _contexto.RegistrosActividad.Add(registro);
            await _contexto.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "No se pudo registrar la actividad de veeduría (AdministradorId: {AdministradorId}, Acción: {Accion}, Módulo: {Modulo}). La operación principal continúa.",
                administradorId,
                accion,
                modulo);
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RegistroActividad>> Listar()
    {
        // Orden descendente por fecha/hora (más reciente primero) para la consulta
        // de solo lectura de la veeduría (Requisitos 5.2, 5.3).
        return await _contexto.RegistrosActividad
            .OrderByDescending(r => r.FechaHora)
            .ToListAsync();
    }
}
