using System;
using System.Collections.Generic;
using System.Linq;
using CsCheck;
using PagWebCabildo.Models.Entidades;
using PagWebCabildo.Services;
using PagWebCabildo.Tests.Infraestructura;
using Xunit;

namespace PagWebCabildo.Tests.Services;

/// <summary>
/// Prueba basada en propiedades de la Property 4 del diseño para el orden del
/// listado de mensajes del panel de administración.
///
/// Validates: Requirements 41.2
///
/// El método bajo prueba es <see cref="MensajeService.ListarRecientesPrimero"/>.
/// Según el Requisito 41.2, el listado de mensajes se muestra ordenado del más
/// reciente al más antiguo (por <see cref="Mensaje.FechaRecepcion"/> descendente).
///
/// La prueba genera en cada iteración un conjunto de mensajes con fechas de
/// recepción variadas que incluyen con frecuencia valores repetidos (se usa una
/// ventana reducida de días y horas), los persiste en una base SQLite en memoria
/// recién creada, invoca el servicio y verifica dos invariantes: (1) cada par
/// adyacente del resultado cumple
/// <c>resultado[i].FechaRecepcion &gt;= resultado[i+1].FechaRecepcion</c> (más
/// reciente primero) y (2) el conjunto devuelto coincide exactamente con el
/// conjunto de mensajes insertados.
/// </summary>
public class MensajeOrdenPropertyTests
{
    private const int IteracionesMinimas = 100;

    // Feature: sitio-web-resguardo, Property 4: Los mensajes se ordenan del más reciente al más antiguo
    [Fact]
    public void ListarRecientesPrimero_OrdenaDescendentePorFechaRecepcion()
    {
        GenMensajes()
            .Sample(mensajes =>
            {
                using var bd = new SqliteEnMemoria();

                // Persistir el conjunto generado en un contexto de escritura.
                using (var ctx = bd.CrearContexto())
                {
                    foreach (var datos in mensajes)
                    {
                        ctx.Mensajes.Add(new Mensaje
                        {
                            Nombre = datos.Nombre,
                            MedioContacto = datos.MedioContacto,
                            Asunto = datos.Asunto,
                            Contenido = datos.Contenido,
                            FechaRecepcion = datos.FechaRecepcion,
                            Atendido = datos.Atendido
                        });
                    }

                    ctx.SaveChanges();
                }

                // Invocar el servicio sobre un contexto de lectura independiente.
                using var ctxLectura = bd.CrearContexto();
                var servicio = new MensajeService(ctxLectura, new ValidacionContactoService());
                var resultado = servicio.ListarRecientesPrimero().GetAwaiter().GetResult();

                // El conjunto devuelto debe coincidir en cantidad con el insertado.
                if (resultado.Count != mensajes.Length)
                {
                    return false;
                }

                // El conjunto devuelto (multiconjunto de fechas) debe ser igual al
                // insertado: no se pierden ni duplican mensajes.
                var fechasEsperadas = mensajes.Select(m => m.FechaRecepcion).OrderBy(f => f).ToList();
                var fechasObtenidas = resultado.Select(m => m.FechaRecepcion).OrderBy(f => f).ToList();
                if (!fechasEsperadas.SequenceEqual(fechasObtenidas))
                {
                    return false;
                }

                // Invariante de orden: descendente por fecha de recepción (más
                // reciente primero), tolerando fechas repetidas con >=.
                for (int i = 0; i + 1 < resultado.Count; i++)
                {
                    if (resultado[i].FechaRecepcion < resultado[i + 1].FechaRecepcion)
                    {
                        return false;
                    }
                }

                return true;
            }, iter: IteracionesMinimas);
    }

    /// <summary>
    /// Datos de un mensaje a sembrar en la prueba.
    /// </summary>
    private readonly record struct DatosMensaje(
        string Nombre,
        string MedioContacto,
        string Asunto,
        string Contenido,
        DateTime FechaRecepcion,
        bool Atendido);

    /// <summary>
    /// Genera una lista de mensajes. Las fechas de recepción se construyen sobre una
    /// ventana reducida de días y horas para forzar con frecuencia valores
    /// repetidos, condición exigida por la tarea.
    /// </summary>
    private static Gen<DatosMensaje[]> GenMensajes()
    {
        var baseFecha = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

        // Ventana pequeña (0-4 días, 0-5 horas) => muchas colisiones de fecha,
        // incluyendo fechas de recepción exactamente repetidas.
        Gen<DateTime> genFecha =
            Gen.Select(Gen.Int[0, 4], Gen.Int[0, 5], (dias, horas) => baseFecha.AddDays(dias).AddHours(horas));

        Gen<bool> genAtendido = Gen.OneOf(Gen.Const(false), Gen.Const(true));

        Gen<DatosMensaje> genMensaje =
            Gen.Select(genFecha, genAtendido, (fecha, atendido) =>
                new DatosMensaje(
                    Nombre: "Visitante",
                    MedioContacto: "visitante@example.com",
                    Asunto: "Asunto",
                    Contenido: "Contenido del mensaje.",
                    FechaRecepcion: fecha,
                    Atendido: atendido));

        // Conjuntos de 0 a 12 mensajes para cubrir listas vacías, de un solo
        // elemento y con muchas colisiones de fecha.
        return genMensaje.Array[0, 12];
    }
}
