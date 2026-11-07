using System;
using System.Collections.Generic;
using System.Linq;
using CsCheck;
using Microsoft.Extensions.Logging.Abstractions;
using PagWebCabildo.Services;
using PagWebCabildo.Tests.Infraestructura;
using Xunit;

namespace PagWebCabildo.Tests.Services;

/// <summary>
/// Prueba basada en propiedades de la Property 3 del diseño para el servicio de
/// registro de actividad (veeduría).
///
/// Validates: Requirements 5.1
///
/// El sistema bajo prueba es <see cref="RegistroActividadService"/>, la fuente de
/// verdad de "cada acción de contenido produce exactamente un registro de
/// actividad": cada operación de crear/editar/eliminar/publicar en los módulos de
/// contenido debe producir exactamente un <c>RegistroActividad</c>, sin omisiones
/// ni duplicados (Requisito 5.1).
///
/// Para cada iteración se genera una secuencia aleatoria (0..12) de acciones de
/// contenido, cada una un tuple de (administradorId, accion, modulo, detalle). Se
/// invoca <c>Registrar</c> una vez por acción sobre una base SQLite en memoria
/// recién creada. Luego se verifica que:
/// 1) el número total de registros persistidos es exactamente igual al número de
///    acciones invocadas (exactamente un registro por acción, sin omisiones ni
///    duplicados);
/// 2) el multiconjunto de tuplas persistidas (admin, accion, modulo, detalle)
///    coincide con el multiconjunto de tuplas invocadas, y toda <c>FechaHora</c>
///    quedó fijada (no es el valor por defecto);
/// 3) <c>Listar()</c> devuelve los registros en orden descendente por
///    <c>FechaHora</c> (monótonamente no creciente).
/// </summary>
public class RegistroActividadPropertyTests
{
    private const int IteracionesMinimas = 100;

    // Feature: roles-cabildo-actas, Property 3: Toda acción de contenido genera un registro de actividad
    [Fact]
    public void Registrar_ProduceExactamenteUnRegistroPorAccionDeContenido()
    {
        GenAcciones()
            .Sample(acciones =>
            {
                using var fixture = new SqliteEnMemoria();

                // Invocar Registrar una vez por acción generada, sobre un contexto
                // de escritura dedicado. RegistroActividadService requiere
                // AppDbContext + ILogger; se usa el logger nulo en pruebas.
                using (var contextoEscritura = fixture.CrearContexto())
                {
                    var servicio = new RegistroActividadService(
                        contextoEscritura,
                        NullLogger<RegistroActividadService>.Instance);

                    foreach (var accion in acciones)
                    {
                        servicio
                            .Registrar(accion.AdministradorId, accion.Accion, accion.Modulo, accion.Detalle)
                            .GetAwaiter()
                            .GetResult();
                    }
                }

                // Releer desde un contexto independiente para verificar lo persistido.
                using var contextoLectura = fixture.CrearContexto();
                var servicioLectura = new RegistroActividadService(
                    contextoLectura,
                    NullLogger<RegistroActividadService>.Instance);

                var persistidos = servicioLectura.Listar().GetAwaiter().GetResult();

                // 1) Exactamente un registro por acción invocada: sin omisiones ni
                //    duplicados.
                if (persistidos.Count != acciones.Count)
                {
                    return false;
                }

                // 2) El multiconjunto de tuplas persistidas coincide con el
                //    multiconjunto de tuplas invocadas.
                var esperadas = acciones
                    .Select(a => (a.AdministradorId, a.Accion, a.Modulo, a.Detalle))
                    .OrderBy(t => t.AdministradorId)
                    .ThenBy(t => t.Accion, StringComparer.Ordinal)
                    .ThenBy(t => t.Modulo, StringComparer.Ordinal)
                    .ThenBy(t => t.Detalle, StringComparer.Ordinal)
                    .ToList();

                var reales = persistidos
                    .Select(r => (r.AdministradorId, r.Accion, r.Modulo, r.Detalle))
                    .OrderBy(t => t.AdministradorId)
                    .ThenBy(t => t.Accion, StringComparer.Ordinal)
                    .ThenBy(t => t.Modulo, StringComparer.Ordinal)
                    .ThenBy(t => t.Detalle, StringComparer.Ordinal)
                    .ToList();

                if (!esperadas.SequenceEqual(reales))
                {
                    return false;
                }

                // Toda FechaHora quedó fijada (no es el valor por defecto).
                if (persistidos.Any(r => r.FechaHora == default))
                {
                    return false;
                }

                // 3) Listar() devuelve los registros en orden descendente por
                //    FechaHora (monótonamente no creciente).
                for (int i = 1; i < persistidos.Count; i++)
                {
                    if (persistidos[i - 1].FechaHora < persistidos[i].FechaHora)
                    {
                        return false;
                    }
                }

                return true;
            }, iter: IteracionesMinimas);
    }

    /// <summary>
    /// Genera una secuencia de 0 a 12 acciones de contenido, para cubrir el caso
    /// vacío (ninguna acción, ningún registro) y secuencias con varias acciones.
    /// </summary>
    private static Gen<List<AccionContenido>> GenAcciones() =>
        GenAccion().List[0, 12].Select(a => a.ToList());

    private static Gen<AccionContenido> GenAccion() =>
        Gen.Select(
            Gen.Int[1, 50],
            GenAccionNombre(),
            GenModulo(),
            Gen.String[0, 30],
            (administradorId, accion, modulo, detalle) =>
                new AccionContenido(administradorId, accion, modulo, detalle));

    /// <summary>
    /// Genera el nombre de la acción de contenido: Crear, Editar, Eliminar o
    /// Publicar.
    /// </summary>
    private static Gen<string> GenAccionNombre() =>
        Gen.OneOf(
            Gen.Const("Crear"),
            Gen.Const("Editar"),
            Gen.Const("Eliminar"),
            Gen.Const("Publicar"));

    /// <summary>
    /// Genera el módulo de contenido sobre el que se realizó la acción.
    /// </summary>
    private static Gen<string> GenModulo() =>
        Gen.OneOf(
            Gen.Const("Comunicado"),
            Gen.Const("Evento"),
            Gen.Const("Autoridad"),
            Gen.Const("Galeria"),
            Gen.Const("Documento"),
            Gen.Const("CategoriaDocumento"),
            Gen.Const("Mensaje"),
            Gen.Const("Acta"),
            Gen.Const("Presentacion"),
            Gen.Const("Contacto"));

    /// <summary>
    /// Acción de contenido generada: identifica al administrador, la acción, el
    /// módulo y el detalle con los que se invoca <c>Registrar</c>.
    /// </summary>
    private readonly record struct AccionContenido(
        int AdministradorId,
        string Accion,
        string Modulo,
        string Detalle);
}
