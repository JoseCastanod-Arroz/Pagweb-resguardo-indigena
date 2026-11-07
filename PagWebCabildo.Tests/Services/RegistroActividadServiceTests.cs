using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using PagWebCabildo.Models.Entidades;
using PagWebCabildo.Services;
using PagWebCabildo.Tests.Infraestructura;
using Xunit;

namespace PagWebCabildo.Tests.Services;

/// <summary>
/// Pruebas unitarias basadas en ejemplos (no en propiedades) de
/// <see cref="RegistroActividadService"/> (implementación de
/// <see cref="IRegistroActividadService"/>), que sustenta la veeduría:
///
/// <list type="bullet">
/// <item>Registrar persiste un <see cref="RegistroActividad"/> con el
/// administrador, la acción, el módulo y el detalle dados, fijando una fecha y
/// hora (Requisito 5.1).</item>
/// <item>Listar devuelve los registros ordenados por fecha y hora descendente,
/// es decir, del más reciente al más antiguo (Requisitos 5.2, 5.3).</item>
/// <item>Registrar es resiliente: una llamada normal no lanza excepción y
/// persiste el registro (manejo de errores del diseño).</item>
/// </list>
///
/// Cada prueba usa una base SQLite en memoria recién creada
/// (<see cref="SqliteEnMemoria"/>) con contextos independientes para escribir y
/// releer. Se emplea <see cref="NullLogger{T}"/> como registrador de logs.
///
/// Validates: Requirements 3.2, 4.1, 5.1
/// </summary>
public sealed class RegistroActividadServiceTests
{
    // -----------------------------------------------------------------------
    // Requisito 5.1: Registrar persiste con fecha/hora establecida.
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Registrar_ConDatosValidos_PersisteRegistroConFechaHora()
    {
        using var bd = new SqliteEnMemoria();
        var antes = DateTime.Now.AddSeconds(-1);

        using (var ctxEscritura = bd.CrearContexto())
        {
            var servicio = new RegistroActividadService(ctxEscritura, NullLogger<RegistroActividadService>.Instance);
            await servicio.Registrar(administradorId: 42, accion: "Crear", modulo: "Acta", detalle: "Acta #1 registrada.");
        }

        var despues = DateTime.Now.AddSeconds(1);

        using (var ctxLectura = bd.CrearContexto())
        {
            var registro = ctxLectura.RegistrosActividad.Single();
            Assert.Equal(42, registro.AdministradorId);
            Assert.Equal("Crear", registro.Accion);
            Assert.Equal("Acta", registro.Modulo);
            Assert.Equal("Acta #1 registrada.", registro.Detalle);

            // La fecha/hora se fijó y cae dentro del intervalo de la operación.
            Assert.NotEqual(default, registro.FechaHora);
            Assert.InRange(registro.FechaHora, antes, despues);
        }
    }

    // -----------------------------------------------------------------------
    // Requisito 5.2 / 5.3: Listar en orden descendente por fecha/hora.
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Listar_DevuelveRegistrosOrdenadosPorFechaHoraDescendente()
    {
        using var bd = new SqliteEnMemoria();

        // Sembrar registros con fechas controladas directamente en el contexto,
        // insertados en desorden para comprobar que el servicio los ordena.
        using (var ctxSemilla = bd.CrearContexto())
        {
            ctxSemilla.RegistrosActividad.AddRange(
                new RegistroActividad { AdministradorId = 1, Accion = "Crear", Modulo = "Comunicado", Detalle = "antiguo", FechaHora = new DateTime(2025, 1, 1, 8, 0, 0) },
                new RegistroActividad { AdministradorId = 1, Accion = "Editar", Modulo = "Evento", Detalle = "reciente", FechaHora = new DateTime(2025, 6, 15, 14, 30, 0) },
                new RegistroActividad { AdministradorId = 2, Accion = "Publicar", Modulo = "Comunicado", Detalle = "intermedio", FechaHora = new DateTime(2025, 3, 10, 9, 0, 0) });
            await ctxSemilla.SaveChangesAsync();
        }

        using (var ctxLectura = bd.CrearContexto())
        {
            var servicio = new RegistroActividadService(ctxLectura, NullLogger<RegistroActividadService>.Instance);
            var listado = await servicio.Listar();

            Assert.Equal(3, listado.Count);

            // El más reciente primero y el más antiguo al final.
            Assert.Equal("reciente", listado[0].Detalle);
            Assert.Equal("intermedio", listado[1].Detalle);
            Assert.Equal("antiguo", listado[2].Detalle);

            // La secuencia de fechas es monótona no creciente.
            for (int i = 1; i < listado.Count; i++)
            {
                Assert.True(listado[i].FechaHora <= listado[i - 1].FechaHora,
                    "Los registros deben estar ordenados por fecha/hora descendente.");
            }
        }
    }

    // -----------------------------------------------------------------------
    // Resiliencia (manejo de errores del diseño): una llamada normal no lanza
    // excepción y persiste. La escritura del registro nunca debe interrumpir la
    // operación principal del llamador.
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Registrar_LlamadaNormal_NoLanzaYPersiste()
    {
        using var bd = new SqliteEnMemoria();

        using var ctx = bd.CrearContexto();
        var servicio = new RegistroActividadService(ctx, NullLogger<RegistroActividadService>.Instance);

        // No debe lanzar ninguna excepción.
        var excepcion = await Record.ExceptionAsync(() =>
            servicio.Registrar(administradorId: 7, accion: "Eliminar", modulo: "Documento", detalle: "Documento eliminado."));
        Assert.Null(excepcion);

        // Y el registro quedó persistido.
        Assert.Equal(1, ctx.RegistrosActividad.Count());
    }
}
