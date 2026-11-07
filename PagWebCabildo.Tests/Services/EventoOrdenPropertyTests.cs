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
/// Prueba basada en propiedades de la Property 1 del diseño para el ordenamiento
/// de eventos por proximidad de fecha.
///
/// Validates: Requirements 5.2
///
/// El servicio bajo prueba es
/// <see cref="EventoService.ListarOrdenadosPorProximidad"/>. Según el Requisito 5.2,
/// la sección de eventos muestra los Eventos ordenados con la fecha más próxima
/// primero; el servicio desempata por hora. La propiedad primaria verificada aquí
/// es que, para todo par adyacente del resultado, la fecha del elemento anterior es
/// menor o igual que la del siguiente (orden ascendente por fecha).
///
/// La prueba genera conjuntos de Eventos con fechas variadas e INCLUYE fechas
/// repetidas (el rango acotado de desplazamiento de días garantiza colisiones) y
/// horas variadas. En cada iteración se usa una base SQLite en memoria fresca, se
/// persisten los eventos, se construye el servicio, se obtiene el listado y se
/// comprueban tanto el orden por fecha como que el conjunto devuelto coincide con el
/// conjunto insertado.
/// </summary>
public class EventoOrdenPropertyTests
{
    private const int IteracionesMinimas = 100;

    // Fecha base a partir de la cual se calculan los desplazamientos de días.
    private static readonly DateOnly FechaBase = new(2024, 1, 1);

    // Feature: sitio-web-resguardo, Property 1: Los eventos se ordenan por proximidad de fecha
    [Fact]
    public void ListarOrdenadosPorProximidad_OrdenaAscendentePorFecha()
    {
        GenEventos()
            .Sample(eventos =>
            {
                using var fixture = new SqliteEnMemoria();

                // Persistir el conjunto generado.
                using (var contexto = fixture.CrearContexto())
                {
                    contexto.Eventos.AddRange(eventos.Select(ClonarSinId));
                    contexto.SaveChanges();
                }

                // Obtener el listado ordenado desde un contexto independiente.
                IReadOnlyList<Evento> resultado;
                using (var contexto = fixture.CrearContexto())
                {
                    var servicio = new EventoService(contexto);
                    resultado = servicio.ListarOrdenadosPorProximidad().GetAwaiter().GetResult();
                }

                // Propiedad primaria (Req 5.2): cada par adyacente está en orden
                // ascendente por fecha.
                for (int i = 0; i + 1 < resultado.Count; i++)
                {
                    if (resultado[i].Fecha > resultado[i + 1].Fecha)
                    {
                        return false;
                    }
                }

                // El conjunto devuelto coincide con el conjunto insertado
                // (misma cardinalidad y mismas claves de contenido).
                if (resultado.Count != eventos.Count)
                {
                    return false;
                }

                var esperados = eventos.Select(Clave).OrderBy(c => c).ToList();
                var obtenidos = resultado.Select(Clave).OrderBy(c => c).ToList();
                return esperados.SequenceEqual(obtenidos);
            }, iter: IteracionesMinimas);
    }

    /// <summary>
    /// Clona un evento generado descartando el Id para que la base lo asigne.
    /// </summary>
    private static Evento ClonarSinId(Evento origen) => new Evento
    {
        Titulo = origen.Titulo,
        Fecha = origen.Fecha,
        Hora = origen.Hora,
        Lugar = origen.Lugar,
        Descripcion = origen.Descripcion,
        ImagenRuta = origen.ImagenRuta,
        Cancelado = origen.Cancelado,
    };

    /// <summary>
    /// Clave de contenido usada para comparar el conjunto insertado con el devuelto
    /// sin depender de los Id asignados por la base.
    /// </summary>
    private static string Clave(Evento e) =>
        $"{e.Titulo}|{e.Fecha:yyyy-MM-dd}|{e.Hora:HH:mm}|{e.Lugar}|{e.Descripcion}|{e.ImagenRuta}|{e.Cancelado}";

    // --- Generadores ---

    /// <summary>
    /// Genera un Evento con fecha a partir de un desplazamiento de días acotado
    /// (para forzar fechas repetidas entre eventos) y hora derivada de minutos
    /// aleatorios del día.
    /// </summary>
    private static Gen<Evento> GenEvento() =>
        Gen.Select(
            Gen.Int[0, 30],            // desplazamiento de días (rango estrecho => fechas repetidas)
            Gen.Int[0, 1439],          // minutos del día (0..23:59)
            Gen.Int[0, 9],             // sufijo para variar el título
            Gen.Bool,                  // cancelado
            (dias, minutos, n, cancelado) => new Evento
            {
                Titulo = $"Evento {n}",
                Fecha = FechaBase.AddDays(dias),
                Hora = new TimeOnly(minutos / 60, minutos % 60),
                Lugar = $"Lugar {n}",
                Descripcion = $"Descripción {n}",
                ImagenRuta = null,
                Cancelado = cancelado,
            });

    /// <summary>
    /// Genera conjuntos de 0 a 12 eventos. El rango acotado de fechas garantiza
    /// fechas repetidas en la mayoría de los conjuntos no triviales.
    /// </summary>
    private static Gen<List<Evento>> GenEventos() =>
        GenEvento().List[0, 12].Select(a => a.ToList());
}
